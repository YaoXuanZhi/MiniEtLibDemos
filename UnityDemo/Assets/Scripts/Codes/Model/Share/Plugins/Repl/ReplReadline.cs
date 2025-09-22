using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ET
{
    /// <summary>
    /// 自定义 Readline：历史导航(↑↓)、Tab 补全、Ghost Text 提示(→ 接受)。
    /// 放在 Model 层因为是纯 UI 输入工具类，无需热更。
    /// </summary>
    public sealed class ReplReadline
    {
        private readonly List<string> _history = new();
        private int _historyIndex = -1;
        private Func<string, string[]>? _completionProvider;

        public void SetCompletionProvider(Func<string, string[]> provider) => _completionProvider = provider;

        public void AddHistory(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return;
            if (_history.Count > 0 && _history[^1] == line) return;
            _history.Add(line);
        }

        public IReadOnlyList<string> History => _history;

        public string ReadLine(string prompt)
        {
            Console.Write(prompt);
            var buf = new StringBuilder();
            int pos = 0;
            _historyIndex = _history.Count;
            string savedInput = "";

            while (true)
            {
                var key = Console.ReadKey(intercept: true);

                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        ClearGhost(buf, pos);
                        Console.WriteLine();
                        var result = buf.ToString();
                        AddHistory(result);
                        return result;

                    case ConsoleKey.Backspace:
                        if (pos > 0)
                        {
                            ClearGhost(buf, pos);
                            buf.Remove(pos - 1, 1);
                            pos--;
                            RedrawLine(prompt, buf, pos);
                        }
                        break;

                    case ConsoleKey.Delete:
                        if (pos < buf.Length)
                        {
                            ClearGhost(buf, pos);
                            buf.Remove(pos, 1);
                            RedrawLine(prompt, buf, pos);
                        }
                        break;

                    case ConsoleKey.LeftArrow:
                        if (pos > 0)
                        {
                            ClearGhost(buf, pos);
                            pos--;
                            Console.SetCursorPosition(prompt.Length + pos, Console.CursorTop);
                        }
                        break;

                    case ConsoleKey.RightArrow:
                        if (pos < buf.Length)
                        {
                            pos++;
                            Console.SetCursorPosition(prompt.Length + pos, Console.CursorTop);
                        }
                        else
                        {
                            var ghost = GetGhostText(buf.ToString());
                            if (ghost != null)
                            {
                                buf.Append(ghost);
                                pos = buf.Length;
                                RedrawLine(prompt, buf, pos);
                            }
                        }
                        break;

                    case ConsoleKey.Home:
                        ClearGhost(buf, pos);
                        pos = 0;
                        Console.SetCursorPosition(prompt.Length, Console.CursorTop);
                        break;

                    case ConsoleKey.End:
                        ClearGhost(buf, pos);
                        pos = buf.Length;
                        Console.SetCursorPosition(prompt.Length + pos, Console.CursorTop);
                        break;

                    case ConsoleKey.UpArrow:
                        if (_history.Count > 0 && _historyIndex > 0)
                        {
                            if (_historyIndex == _history.Count)
                                savedInput = buf.ToString();
                            _historyIndex--;
                            ClearGhost(buf, pos);
                            buf.Clear();
                            buf.Append(_history[_historyIndex]);
                            pos = buf.Length;
                            RedrawLine(prompt, buf, pos);
                        }
                        break;

                    case ConsoleKey.DownArrow:
                        if (_historyIndex < _history.Count)
                        {
                            _historyIndex++;
                            ClearGhost(buf, pos);
                            buf.Clear();
                            buf.Append(_historyIndex < _history.Count ? _history[_historyIndex] : savedInput);
                            pos = buf.Length;
                            RedrawLine(prompt, buf, pos);
                        }
                        break;

                    case ConsoleKey.Tab:
                        var completions = _completionProvider?.Invoke(buf.ToString());
                        if (completions is { Length: > 0 })
                        {
                            ClearGhost(buf, pos);
                            if (completions.Length == 1)
                            {
                                buf.Clear();
                                buf.Append(completions[0]);
                                if (!completions[0].EndsWith(' '))
                                    buf.Append(' ');
                                pos = buf.Length;
                                RedrawLine(prompt, buf, pos);
                            }
                            else
                            {
                                Console.WriteLine();
                                foreach (var c in completions.Take(20))
                                    Console.Write($"  {c}");
                                Console.WriteLine();
                                Console.Write(prompt);
                                Console.Write(buf);
                                pos = buf.Length;
                                Console.SetCursorPosition(prompt.Length + pos, Console.CursorTop);
                            }
                        }
                        break;

                    case ConsoleKey.Escape:
                        ClearGhost(buf, pos);
                        buf.Clear();
                        pos = 0;
                        RedrawLine(prompt, buf, pos);
                        break;

                    default:
                        if (key.KeyChar >= 32)
                        {
                            ClearGhost(buf, pos);
                            buf.Insert(pos, key.KeyChar);
                            pos++;
                            RedrawLine(prompt, buf, pos);
                        }
                        break;
                }

                ShowGhost(prompt, buf, pos);
            }
        }

        private string? GetGhostText(string current)
        {
            if (string.IsNullOrEmpty(current)) return null;

            for (int i = _history.Count - 1; i >= 0; i--)
            {
                if (_history[i].StartsWith(current, StringComparison.OrdinalIgnoreCase) && _history[i].Length > current.Length)
                    return _history[i][current.Length..];
            }

            var completions = _completionProvider?.Invoke(current);
            if (completions is { Length: > 0 })
            {
                var match = completions.FirstOrDefault(c =>
                    c.StartsWith(current, StringComparison.OrdinalIgnoreCase) && c.Length > current.Length);
                if (match != null)
                    return match[current.Length..];
            }

            return null;
        }

        private void ShowGhost(string prompt, StringBuilder buf, int pos)
        {
            if (pos != buf.Length) return;
            var ghost = GetGhostText(buf.ToString());
            if (ghost == null) return;

            var origColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(ghost);
            Console.ForegroundColor = origColor;
            Console.SetCursorPosition(prompt.Length + pos, Console.CursorTop);
        }

        private void ClearGhost(StringBuilder buf, int pos)
        {
            if (pos == buf.Length)
            {
                var ghost = GetGhostText(buf.ToString());
                if (ghost != null)
                {
                    var curLeft = Console.CursorLeft;
                    var curTop = Console.CursorTop;
                    Console.Write(new string(' ', ghost.Length));
                    Console.SetCursorPosition(curLeft, curTop);
                }
            }
        }

        private static void RedrawLine(string prompt, StringBuilder buf, int pos)
        {
            var top = Console.CursorTop;
            Console.SetCursorPosition(0, top);
            var line = prompt + buf;
            Console.Write(line);
            var totalWidth = Console.BufferWidth;
            var remaining = totalWidth - line.Length % totalWidth;
            if (remaining > 0 && remaining < totalWidth)
                Console.Write(new string(' ', remaining));
            Console.SetCursorPosition(prompt.Length + pos, top);
        }
    }
}

