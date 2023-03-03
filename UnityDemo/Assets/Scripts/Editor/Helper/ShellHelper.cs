using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ET
{
    public static class ShellHelper
    {
        public static void Run(string cmd, string workDirectory, List<string> environmentVars = null)
        {
            Process process = new();
            try
            {
#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
                string app = "bash";
                string splitChar = ":";
                string arguments = "-c";
#elif UNITY_EDITOR_WIN
                string app = "cmd.exe";
                string splitChar = ";";
                string arguments = "/c";
#endif
                ProcessStartInfo start = new ProcessStartInfo(app);

                if (environmentVars != null)
                {
                    foreach (string var in environmentVars)
                    {
                        start.EnvironmentVariables["PATH"] += (splitChar + var);
                    }
                }

                process.StartInfo = start;
                start.Arguments = arguments + " \"" + cmd + "\"";
                start.CreateNoWindow = true;
                start.ErrorDialog = true;
                start.UseShellExecute = false;
                start.WorkingDirectory = workDirectory;

                if (start.UseShellExecute)
                {
                    start.RedirectStandardOutput = false;
                    start.RedirectStandardError = false;
                    start.RedirectStandardInput = false;
                }
                else
                {
                    start.RedirectStandardOutput = true;
                    start.RedirectStandardError = true;
                    start.RedirectStandardInput = true;
                    start.StandardOutputEncoding = System.Text.Encoding.UTF8;
                    start.StandardErrorEncoding = System.Text.Encoding.UTF8;
                }

                bool endOutput = false;
                bool endError = false;

                process.OutputDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                    {
                        UnityEngine.Debug.Log(args.Data);
                    }
                    else
                    {
                        endOutput = true;
                    }
                };

                process.ErrorDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                    {
                        UnityEngine.Debug.LogError(args.Data);
                    }
                    else
                    {
                        endError = true;
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                while (!endOutput || !endError)
                {
                }

                process.CancelOutputRead();
                process.CancelErrorRead();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
            finally
            {
                process.Close();
            }
        }

        /// <summary>
        /// 相当于调用了exec_name args
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="result"></param>
        /// <param name="isLogError"></param>
        /// <param name="arg"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static bool Cmd(string cmd, out string result, bool isLogError, string arg, Encoding encoding = null)
        {
            result = string.Empty;
            try
            {
                Process p = new Process();
                p.StartInfo.FileName = cmd;
                p.StartInfo.Arguments = arg;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.CreateNoWindow = true;

                if (encoding != null)
                {
                    p.StartInfo.StandardInputEncoding = encoding;
                    p.StartInfo.StandardOutputEncoding = encoding;
                    p.StartInfo.StandardErrorEncoding = encoding;
                }

                p.Start();
                result = p.StandardOutput.ReadToEnd() + p.StandardError.ReadToEnd();
                p.Close();
                return true;
            }
            catch (System.Exception ex)
            {
                if (isLogError)
                    UnityEngine.Debug.LogError(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 相当于调用了exec_name args，会根据当前cmd的代码页自动识别，解决非ASCII字符乱码问题
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="result"></param>
        /// <param name="isLogError"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static bool CmdEx(string cmd, out string result, bool isLogError, string arg)
        {
            Cmd("cmd", out var output, false, "/c chcp");
            var match = Regex.Match(output, @"(\d+)");
            int codePage = 65001;
            if (match.Length > 0)
                codePage = Convert.ToInt32(match.Value);

            return Cmd(cmd, out result, isLogError, arg, Encoding.GetEncoding(codePage));
        }

        /// <summary>
        /// 相当于调用了start exec_name args
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="result"></param>
        /// <param name="isLogError"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static bool StartCmd(string cmd, out string result, bool isLogError, string arg)
        {
            result = string.Empty;
            try
            {
                Process p = new Process();
                p.StartInfo.FileName = cmd;
                p.StartInfo.Arguments = arg;
                p.Start();
                result = p.StandardOutput.ReadToEnd() + p.StandardError.ReadToEnd();
                p.Close();
                return true;
            }
            catch (System.Exception ex)
            {
                if (isLogError)
                    UnityEngine.Debug.LogError(ex.ToString());
                return false;
            }
        }
    }
}