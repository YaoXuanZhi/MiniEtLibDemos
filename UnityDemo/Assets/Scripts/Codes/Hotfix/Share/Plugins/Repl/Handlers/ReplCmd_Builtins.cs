using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ET
{
    [ReplCommand("help", "显示可用命令列表")]
    public class ReplCmd_Help : IReplCommandHandler
    {
        public async ETTask Run(ReplComponent repl, string content, string[] args)
        {
            
            Console.WriteLine("┌──────────────────────────┬──────────────────────────────────────┐");
            Console.WriteLine("│ Command                  │ Description                          │");
            Console.WriteLine("├──────────────────────────┼──────────────────────────────────────┤");

            foreach (var kv in repl.CommandDescriptions.OrderBy(k => k.Key))
            {
                Console.WriteLine($"│ {kv.Key,-24} │ {kv.Value,-36} │");
            }

            Console.WriteLine("│ <Method> [args]          │ Call method on selected entity        │");
            Console.WriteLine("└──────────────────────────┴──────────────────────────────────────┘");
            await ETTask.CompletedTask;
        }
    }

    [ReplCommand("clear", "清空控制台")]
    public class ReplCmd_Clear : IReplCommandHandler
    {
        public async ETTask Run(ReplComponent repl, string content, string[] args)
        {
            Console.Clear();
            await ETTask.CompletedTask;
        }
    }

    [ReplCommand("history", "显示命令历史")]
    public class ReplCmd_History : IReplCommandHandler
    {
        public async ETTask Run(ReplComponent repl, string content, string[] args)
        {
            for (int i = 0; i < repl.History.Count; i++)
                Console.WriteLine($"  {i + 1}. {repl.History[i]}");
            await ETTask.CompletedTask;
        }
    }

    [ReplCommand("list", "列出活跃的场景/实体")]
    public class ReplCmd_List : IReplCommandHandler
    {
        public async ETTask Run(ReplComponent repl, string content, string[] args)
        {
            var entities = ReplComponentSystem.GetAllEntities();
            string? filterType = args.Length > 0 ? args[0] : null;

            var scenes = entities.Values
                .OfType<Scene>()
                .Where(s => filterType == null ||
                            s.SceneType.ToString().Contains(filterType, StringComparison.OrdinalIgnoreCase) ||
                            (s.Name != null && s.Name.Contains(filterType, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (scenes.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("No scenes found.");
                Console.ResetColor();

                if (filterType != null)
                {
                    var others = entities.Values
                        .Where(e => e.GetType().Name.Contains(filterType, StringComparison.OrdinalIgnoreCase))
                        .Take(30).ToList();
                    if (others.Count > 0)
                    {
                        Console.WriteLine($"{"InstanceId",-18} {"Type",-30} {"Id",-18}");
                        Console.WriteLine(new string('─', 70));
                        foreach (var e in others)
                            Console.WriteLine($"{e.InstanceId,-18} {e.GetType().Name,-30} {e.Id,-18}");
                    }
                }
                await ETTask.CompletedTask;
                return;
            }

            Console.WriteLine($"{"Id",-12} {"InstanceId",-18} {"Name",-20} {"SceneType",-15} {"Zone",-6} {"Comps",-5}");
            Console.WriteLine(new string('─', 80));
            foreach (var s in scenes)
            {
                var compCount = 0;
                try { compCount = s.Components.Count; } catch { }
                Console.WriteLine($"{s.Id,-12} {s.InstanceId,-18} {s.Name ?? "",-20} {s.SceneType,-15} {s.Zone,-6} {compCount,-5}");
            }
            Console.WriteLine($"Total: {scenes.Count} scene(s)");
            await ETTask.CompletedTask;
        }
    }

    [ReplCommand("select", "通过名称或ID选中场景/实体")]
    public class ReplCmd_Select : IReplCommandHandler
    {
        public async ETTask Run(ReplComponent repl, string content, string[] args)
        {
            var entities = ReplComponentSystem.GetAllEntities();
            var scenes = entities.Values.OfType<Scene>().ToList();

            if (args.Length == 0)
            {
                if (scenes.Count == 0) { ReplComponentSystem.PrintError("No scenes available."); return; }

                Console.WriteLine("Available scenes:");
                for (int i = 0; i < scenes.Count; i++)
                    Console.WriteLine($"  {i + 1}. {scenes[i].Name} (Id={scenes[i].Id}, {scenes[i].SceneType})");
                Console.Write("Enter number: ");
                var line = Console.ReadLine();
                if (int.TryParse(line, out var idx) && idx >= 1 && idx <= scenes.Count)
                {
                    var s = scenes[idx - 1];
                    repl.SelectedEntityId = s.InstanceId;
                    repl.SelectedLabel = s.Name ?? s.SceneType.ToString();
                    ReplComponentSystem.PrintSuccess($"Selected: {repl.SelectedLabel}");
                }
                await ETTask.CompletedTask;
                return;
            }

            var query = args[0];
            if (long.TryParse(query, out var id))
            {
                var match = scenes.FirstOrDefault(s => s.Id == id || s.InstanceId == id);
                if (match == null)
                {
                    if (entities.TryGetValue(id, out var entity))
                    {
                        repl.SelectedEntityId = entity.InstanceId;
                        repl.SelectedLabel = entity.GetType().Name + "#" + entity.InstanceId;
                        ReplComponentSystem.PrintSuccess($"Selected entity: {repl.SelectedLabel}");
                        return;
                    }
                    ReplComponentSystem.PrintError($"Not found: {id}");
                    return;
                }
                repl.SelectedEntityId = match.InstanceId;
                repl.SelectedLabel = match.Name ?? match.SceneType.ToString();
                ReplComponentSystem.PrintSuccess($"Selected: {repl.SelectedLabel}");
                return;
            }

            var byName = scenes.FirstOrDefault(s => string.Equals(s.Name, query, StringComparison.OrdinalIgnoreCase))
                      ?? scenes.FirstOrDefault(s => s.Name != null && s.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
            if (byName != null)
            {
                repl.SelectedEntityId = byName.InstanceId;
                repl.SelectedLabel = byName.Name ?? byName.SceneType.ToString();
                ReplComponentSystem.PrintSuccess($"Selected: {repl.SelectedLabel}");
            }
            else
            {
                ReplComponentSystem.PrintError($"No scene found matching '{query}'");
            }
            await ETTask.CompletedTask;
        }
    }

    [ReplCommand("deselect", "取消选中当前实体")]
    public class ReplCmd_Deselect : IReplCommandHandler
    {
        public async ETTask Run(ReplComponent repl, string content, string[] args)
        {
            repl.SelectedEntityId = 0;
            repl.SelectedLabel = "";
            ReplComponentSystem.PrintSuccess("Deselected.");
            await ETTask.CompletedTask;
        }
    }

    [ReplCommand("info", "显示选中实体的详细信息")]
    public class ReplCmd_Info : IReplCommandHandler
    {
        public async ETTask Run(ReplComponent repl, string content, string[] args)
        {
            var entity = repl.GetSelectedEntity();
            if (entity == null) { ReplComponentSystem.PrintError("No entity selected. Use 'select' first."); return; }

            var info = new Dictionary<string, object?>
            {
                ["Type"] = entity.GetType().FullName,
                ["Id"] = entity.Id,
                ["InstanceId"] = entity.InstanceId,
                ["IsDisposed"] = entity.IsDisposed,
            };

            if (entity is Scene s)
            {
                info["Name"] = s.Name;
                info["SceneType"] = s.SceneType.ToString();
                info["Zone"] = s.Zone;
            }

            try { info["Components"] = string.Join(", ", entity.Components.Keys.Select(t => t.Name)); } catch { }
            try { info["ChildrenCount"] = entity.Children.Count; } catch { }

            ReplComponentSystem.PrintJson("Entity Info", info);
            await ETTask.CompletedTask;
        }
    }

    [ReplCommand("methods", "列出选中实体的可调用方法")]
    public class ReplCmd_Methods : IReplCommandHandler
    {
        public async ETTask Run(ReplComponent repl, string content, string[] args)
        {
            var entity = repl.GetSelectedEntity();
            if (entity == null) { ReplComponentSystem.PrintError("No entity selected. Use 'select' first."); return; }

            var methods = repl.GetMethodEntries(entity.GetType());
            if (methods.Length == 0) { Console.WriteLine("No callable methods found."); return; }

            Console.WriteLine($"{"Method",-25} {"Parameters",-40} {"Returns",-15} {"Async",-5}");
            Console.WriteLine(new string('─', 90));
            foreach (var m in methods.OrderBy(m => m.Name))
            {
                var parms = string.Join(", ", m.Parameters.Select(p => $"{p.ParameterType.Name} {p.Name}"));
                Console.WriteLine($"{m.Name,-25} {parms,-40} {m.Info.ReturnType.Name,-15} {(m.IsAsync ? "✓" : ""),-5}");
            }
            await ETTask.CompletedTask;
        }
    }

    [ReplCommand("tree", "显示实体树形结构")]
    public class ReplCmd_Tree : IReplCommandHandler
    {
        public async ETTask Run(ReplComponent repl, string content, string[] args)
        {
            var root = Root.Instance;
            if (root?.Scene == null) { ReplComponentSystem.PrintError("Root.Instance is null."); return; }

            Console.WriteLine("Root");
            PrintTree(root.Scene, "  ", true);
            await ETTask.CompletedTask;
        }

        private static void PrintTree(Entity entity, string indent, bool last)
        {
            var prefix = last ? "└─ " : "├─ ";
            var label = entity is Scene s
                ? $"Scene:{s.Name ?? s.SceneType.ToString()} Id={s.Id}"
                : $"{entity.GetType().Name} Id={entity.Id}";
            Console.WriteLine($"{indent}{prefix}{label}");

            var newIndent = indent + (last ? "   " : "│  ");
            var items = new List<Entity>();
            try { items.AddRange(entity.Components.Values); } catch { }
            try { items.AddRange(entity.Children.Values); } catch { }

            for (int i = 0; i < items.Count && i < 50; i++)
                PrintTree(items[i], newIndent, i == items.Count - 1);
        }
    }

    [ReplCommand("stats", "GC/内存/实体统计信息")]
    public class ReplCmd_Stats : IReplCommandHandler
    {
        public async ETTask Run(ReplComponent repl, string content, string[] args)
        {
            var entities = ReplComponentSystem.GetAllEntities();
            var process = Process.GetCurrentProcess();
            var stats = new Dictionary<string, object>
            {
                ["Total Entities"] = entities.Count,
                ["Scene Count"] = entities.Values.OfType<Scene>().Count(),
                ["GC Gen0"] = GC.CollectionCount(0),
                ["GC Gen1"] = GC.CollectionCount(1),
                ["GC Gen2"] = GC.CollectionCount(2),
                ["GC Heap"] = $"{GC.GetTotalMemory(false) / 1024.0 / 1024.0:F2} MB",
                ["Working Set"] = $"{process.WorkingSet64 / 1024.0 / 1024.0:F2} MB",
                ["Threads"] = process.Threads.Count,
                ["Uptime"] = (DateTime.Now - process.StartTime).ToString(@"dd\.hh\:mm\:ss"),
            };

            var typeCounts = entities.Values
                .GroupBy(e => e.GetType().Name)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => $"{g.Key}:{g.Count()}")
                .ToList();
            stats["Top Types"] = string.Join(", ", typeCounts);

            ReplComponentSystem.PrintJson("System Stats", stats);
            await ETTask.CompletedTask;
        }
    }

    [ReplCommand("gc", "强制垃圾回收")]
    public class ReplCmd_Gc : IReplCommandHandler
    {
        public async ETTask Run(ReplComponent repl, string content, string[] args)
        {
            var before = GC.GetTotalMemory(false);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var after = GC.GetTotalMemory(true);
            ReplComponentSystem.PrintSuccess($"GC done. {before / 1048576.0:F2} MB → {after / 1048576.0:F2} MB (freed {(before - after) / 1048576.0:F2} MB)");
            await ETTask.CompletedTask;
        }
    }

    [ReplCommand("shutdown", "关闭服务器（需二次确认）")]
    public class ReplCmd_Shutdown : IReplCommandHandler
    {
        public async ETTask Run(ReplComponent repl, string content, string[] args)
        {
            Console.Write("Are you sure you want to shutdown? (y/N): ");
            var line = Console.ReadLine();
            if (line?.Trim().ToLower() == "y")
            {
                Console.WriteLine("Shutting down...");
                Environment.Exit(0);
            }
            else
            {
                Console.WriteLine("Cancelled.");
            }
            await ETTask.CompletedTask;
        }
    }
}

