using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Text.Json;
using ET.Server;

namespace ET
{
    [FriendOf(typeof(ReplComponent))]
    public static class ReplComponentSystem
    {
        [ObjectSystem]
        public class ReplComponentAwakeSystem : AwakeSystem<ReplComponent>
        {
            protected override void Awake(ReplComponent self)
            {
                self.Running = true;
                self.History.Clear();
                self.MethodCache.Clear();
                self.LoadHandlers();
                self.StartRepl();
            }
        }

        [ObjectSystem]
        public class ReplComponentDestroySystem : DestroySystem<ReplComponent>
        {
            protected override void Destroy(ReplComponent self)
            {
                self.Running = false;
            }
        }

        [ObjectSystem]
        public class ReplComponentLoadSystem : LoadSystem<ReplComponent>
        {
            protected override void Load(ReplComponent self)
            {
                self.LoadHandlers();
                Log.Console("[REPL] Handlers reloaded after hotfix.");
            }
        }

        #region Handler Discovery

        /// <summary>
        /// 通过反射扫描所有标记了 [ReplCommand] 的 IReplCommandHandler，构建 Handlers 字典。
        /// 在 Awake 和 Load（热更后）时调用。
        /// </summary>
        public static void LoadHandlers(this ReplComponent self)
        {
            self.Handlers.Clear();
            self.CommandDescriptions.Clear();

            HashSet<Type> types = EventSystem.Instance.GetTypes(typeof(ReplCommandAttribute));

            foreach (Type type in types)
            {
                object[] attrs = type.GetCustomAttributes(typeof(ReplCommandAttribute), false);
                if (attrs.Length == 0) continue;

                var attr = (ReplCommandAttribute)attrs[0];

                object obj = Activator.CreateInstance(type);
                if (obj is not IReplCommandHandler handler)
                {
                    Log.Error($"[REPL] Type {type.FullName} has [ReplCommand] but does not implement IReplCommandHandler");
                    continue;
                }

                var cmd = attr.Command.ToLowerInvariant();
                self.Handlers[cmd] = handler;
                if (!string.IsNullOrEmpty(attr.Description))
                    self.CommandDescriptions[cmd] = attr.Description;
            }

            Log.Debug($"[REPL] Loaded {self.Handlers.Count} command handlers.");
        }

        #endregion

        #region Startup

        public static void StartRepl(this ReplComponent self)
        {
            self.StartReplAsync().Coroutine();
        }

        private static async ETTask StartReplAsync(this ReplComponent self)
        {
            await TimerComponent.Instance.WaitAsync(500);

            await Task.Run(async () =>
            {
                try
                {
                    await self.RunLoopAsync();
                }
                catch (Exception e)
                {
                    Log.Error($"REPL error: {e}");
                }
            });
        }

        #endregion

        #region Main Loop

        private static async Task RunLoopAsync(this ReplComponent self)
        {
            var readline = new ReplReadline();
            readline.SetCompletionProvider(input => self.GetCompletions(input));

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("═══ ET REPL Console ═══  Type 'help' for commands.");
            Console.ResetColor();

            while (self.Running && !self.IsDisposed)
            {
                try
                {
                    var rawPrompt = self.SelectedEntityId != 0
                        ? $"[{self.SelectedLabel}] > "
                        : "repl> ";

                    var input = readline.ReadLine(rawPrompt);
                    if (string.IsNullOrWhiteSpace(input)) continue;

                    self.History.Add(input);
                    var parts = ParseArgs(input);
                    if (parts.Length == 0) continue;

                    var cmd = parts[0].ToLowerInvariant();
                    var args = parts[1..];

                    // 优先查找注册的命令处理器
                    if (self.Handlers.TryGetValue(cmd, out var handler))
                    {
                        await handler.Run(self, input, args);
                        continue;
                    }

                    // 如果有选中的 Entity，尝试方法调用
                    if (self.SelectedEntityId != 0)
                    {
                        await self.CmdInvokeMethod(parts[0], args);
                    }
                    else
                    {
                        PrintError($"Unknown command: {cmd}. Type 'help' for available commands.");
                    }
                }
                catch (Exception ex)
                {
                    PrintException(ex);
                }
            }
        }

        #endregion

        #region Method Invocation (fallback for selected entity)

        private static async Task CmdInvokeMethod(this ReplComponent self, string methodName, string[] args)
        {
            var entity = self.GetSelectedEntity();
            if (entity == null) { PrintError("Selected entity is gone."); return; }

            var entry = self.FindMethod(entity.GetType(), methodName);
            if (entry == null)
            {
                PrintError($"Unknown command or method: {methodName}. Type 'methods' to see available methods.");
                return;
            }

            var paramInfos = entry.Parameters;
            var invokeArgs = new object?[paramInfos.Length];

            for (int i = 0; i < paramInfos.Length; i++)
            {
                if (i < args.Length)
                {
                    try { invokeArgs[i] = ConvertArg(args[i], paramInfos[i].ParameterType); }
                    catch (Exception ex) { PrintError($"Arg convert failed '{args[i]}' → {paramInfos[i].ParameterType.Name}: {ex.Message}"); return; }
                }
                else if (paramInfos[i].HasDefaultValue)
                {
                    invokeArgs[i] = paramInfos[i].DefaultValue;
                }
                else
                {
                    Console.Write($"  {paramInfos[i].ParameterType.Name} {paramInfos[i].Name}: ");
                    var val = Console.ReadLine() ?? "";
                    try { invokeArgs[i] = ConvertArg(val, paramInfos[i].ParameterType); }
                    catch (Exception ex) { PrintError($"Conversion error: {ex.Message}"); return; }
                }
            }

            try
            {
                var raw = entry.Info.Invoke(entity, invokeArgs);
                var result = await UnwrapAsyncResult(raw, entry.Info.ReturnType, entry.IsAsync);
                if (result != null)
                    PrintJson($"{entry.Name} Result", result);
                else
                    PrintSuccess($"{entry.Name} executed (void/null).");
            }
            catch (TargetInvocationException tie)
            {
                PrintException(tie.InnerException ?? tie);
            }
            catch (Exception ex)
            {
                PrintException(ex);
            }
        }

        #endregion

        #region Method Cache

        private static readonly HashSet<string> ExcludedMethods = new(StringComparer.Ordinal)
        {
            "ToString", "GetHashCode", "Equals", "GetType",
            "Dispose", "BeforeSerialize", "AfterDeserialize",
            "AddComponent", "AddComponentWithId", "RemoveComponent", "GetComponent",
            "AddChild", "AddChildWithId", "GetChild", "RemoveChild",
            "GetParent", "MemberwiseClone", "Finalize",
        };

        public static ReplMethodEntry[] GetMethodEntries(this ReplComponent self, Type type)
        {
            return self.MethodCache.GetOrAdd(type, static t =>
            {
                var list = new List<ReplMethodEntry>();
                MethodInfo[] methods;
                try { methods = t.GetMethods(BindingFlags.Instance | BindingFlags.Public); }
                catch { return Array.Empty<ReplMethodEntry>(); }

                foreach (var mi in methods)
                {
                    try
                    {
                        if (mi.IsSpecialName || mi.IsGenericMethod) continue;
                        if (ExcludedMethods.Contains(mi.Name)) continue;
                        if (mi.GetParameters().Any(p => p.IsOut || p.ParameterType.IsByRef)) continue;

                        list.Add(new ReplMethodEntry
                        {
                            Name = mi.Name,
                            Info = mi,
                            Parameters = mi.GetParameters(),
                            IsAsync = IsAsyncReturn(mi.ReturnType),
                        });
                    }
                    catch { }
                }

                return list.ToArray();
            });
        }

        public static ReplMethodEntry? FindMethod(this ReplComponent self, Type type, string name)
        {
            return self.GetMethodEntries(type).FirstOrDefault(m =>
                string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsAsyncReturn(Type t)
        {
            if (t == typeof(Task) || (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Task<>)))
                return true;
            if (t == typeof(ETTask))
                return true;
            var fullName = t.IsGenericType ? t.GetGenericTypeDefinition().FullName : t.FullName;
            return fullName != null && fullName.StartsWith("ET.ETTask");
        }

        #endregion

        #region Async Unwrap

        public static async Task<object?> UnwrapAsyncResult(object? raw, Type returnType, bool isAsync)
        {
            if (!isAsync || raw == null) return raw;

            if (raw is ETTask etTask)
            {
                await etTask;
                return null;
            }

            // ETTask<T>
            var typeName = returnType.IsGenericType ? returnType.GetGenericTypeDefinition().FullName : returnType.FullName;
            if (typeName != null && typeName.StartsWith("ET.ETTask`1"))
            {
                var awaiter = returnType.GetMethod("GetAwaiter")!.Invoke(raw, null)!;
                var awaiterType = awaiter.GetType();
                var isCompleted = (bool)awaiterType.GetProperty("IsCompleted")!.GetValue(awaiter)!;
                if (!isCompleted)
                {
                    var tcs = new TaskCompletionSource<object?>();
                    var onCompleted = awaiterType.GetMethod("OnCompleted") ?? awaiterType.GetMethod("UnsafeOnCompleted");
                    if (onCompleted != null)
                    {
                        onCompleted.Invoke(awaiter, new object[]
                        {
                            (Action)(() =>
                            {
                                try { tcs.SetResult(awaiterType.GetMethod("GetResult")!.Invoke(awaiter, null)); }
                                catch (Exception ex) { tcs.SetException(ex); }
                            })
                        });
                        return await tcs.Task;
                    }
                }
                return awaiterType.GetMethod("GetResult")!.Invoke(awaiter, null);
            }

            if (raw is Task task)
            {
                await task;
                var taskType = raw.GetType();
                if (taskType.IsGenericType)
                    return taskType.GetProperty("Result")!.GetValue(raw);
                return null;
            }

            return raw;
        }

        #endregion

        #region Arg Conversion

        public static object? ConvertArg(string raw, Type targetType)
        {
            if (targetType == typeof(string)) return raw;
            if (targetType == typeof(int)) return int.Parse(raw);
            if (targetType == typeof(long)) return long.Parse(raw);
            if (targetType == typeof(float)) return float.Parse(raw);
            if (targetType == typeof(double)) return double.Parse(raw);
            if (targetType == typeof(bool)) return bool.Parse(raw);
            if (targetType == typeof(short)) return short.Parse(raw);
            if (targetType == typeof(byte)) return byte.Parse(raw);
            if (targetType == typeof(List<string>)) return raw.Split(',').ToList();
            if (targetType == typeof(List<int>)) return raw.Split(',').Select(int.Parse).ToList();
            if (targetType == typeof(List<long>)) return raw.Split(',').Select(long.Parse).ToList();
            if (targetType.IsEnum) return Enum.Parse(targetType, raw, ignoreCase: true);
            return JsonSerializer.Deserialize(raw, targetType);
        }

        #endregion

        #region Public Helpers (available to command handlers)

        public static Entity? GetSelectedEntity(this ReplComponent self)
        {
            if (self.SelectedEntityId == 0) return null;
            return Root.Instance?.Get(self.SelectedEntityId);
        }

        public static Dictionary<long, Entity> GetAllEntities()
        {
            var root = Root.Instance;
            if (root == null) return new Dictionary<long, Entity>();
            var field = typeof(Root).GetField("allEntities", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field?.GetValue(root) is Dictionary<long, Entity> dict)
                return dict;
            return new Dictionary<long, Entity>();
        }

        public static GmDispatcherComponent? GetGmDispatcher()
        {
            var root = Root.Instance;
            if (root?.Scene == null) return null;
            try
            {
                foreach (var kv in root.Scene.Components)
                {
                    if (kv.Value is GmDispatcherComponent gm)
                        return gm;
                }
            }
            catch { }
            return null;
        }

        public static void PrintSuccess(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("✓ ");
            Console.ResetColor();
            Console.WriteLine(msg);
        }

        public static void PrintError(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        public static void PrintException(Exception ex)
        {
            var inner = ex is TargetInvocationException tie ? (tie.InnerException ?? ex) : ex;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{inner.GetType().Name}] {inner.Message}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(inner.StackTrace);
            Console.ResetColor();
        }

        public static void PrintJson(string title, object obj)
        {
            var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"── {title} ──");
            Console.ResetColor();
            Console.WriteLine(json);
        }

        #endregion

        #region Completions

        private static string[] GetCompletions(this ReplComponent self, string input)
        {
            if (string.IsNullOrEmpty(input))
                return self.Handlers.Keys.ToArray();

            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length <= 1)
            {
                var prefix = input.TrimStart();
                var results = new List<string>();
                results.AddRange(self.Handlers.Keys.Where(c => c.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)));

                var entity = self.GetSelectedEntity();
                if (entity != null)
                {
                    var methods = self.GetMethodEntries(entity.GetType());
                    results.AddRange(methods
                        .Where(m => m.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                        .Select(m => m.Name));
                }
                return results.Distinct().ToArray();
            }

            var cmd = parts[0].ToLowerInvariant();

            if (cmd == "select")
            {
                var entities = GetAllEntities();
                var prefix = parts.Length > 1 ? parts[1] : "";
                return entities.Values.OfType<Scene>()
                    .Where(s => s.Name != null && s.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    .Select(s => $"select {s.Name}")
                    .ToArray();
            }

            if (cmd == "gm")
            {
                var dispatcher = GetGmDispatcher();
                if (dispatcher != null)
                {
                    var prefix = parts.Length > 1 ? parts[1] : "";
                    return dispatcher.Handlers.Keys
                        .Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                        .Select(k => $"gm {k}")
                        .ToArray();
                }
            }

            return Array.Empty<string>();
        }

        #endregion

        #region Parse

        public static string[] ParseArgs(string input)
        {
            var args = new List<string>();
            var current = new System.Text.StringBuilder();
            bool inQuote = false;
            char quoteChar = '"';

            foreach (var ch in input)
            {
                if (inQuote)
                {
                    if (ch == quoteChar) inQuote = false;
                    else current.Append(ch);
                }
                else if (ch == '"' || ch == '\'')
                {
                    inQuote = true;
                    quoteChar = ch;
                }
                else if (ch == ' ')
                {
                    if (current.Length > 0) { args.Add(current.ToString()); current.Clear(); }
                }
                else current.Append(ch);
            }
            if (current.Length > 0) args.Add(current.ToString());
            return args.ToArray();
        }

        #endregion
    }
}

