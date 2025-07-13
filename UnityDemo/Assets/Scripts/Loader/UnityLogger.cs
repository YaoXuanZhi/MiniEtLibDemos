using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace ET
{
    public partial class UnityLogger: ILog
    {
        public void Trace(string msg)
        {
            LogImp(ELogLevel.Info, msg);
        }

        public void Debug(string msg)
        {
            LogImp(ELogLevel.Debug, msg);
        }

        public void Info(string msg)
        {
            LogImp(ELogLevel.Info, msg);
        }

        public void Warning(string msg)
        {
            LogImp(ELogLevel.Warning, msg);
        }

        public void Error(string msg)
        {
            LogImp(ELogLevel.Error, msg);
        }

        public void Error(Exception e)
        {
            LogImp(ELogLevel.Exception, e.ToString());
        }

        public void Trace(string message, params object[] args)
        {
            LogImp(ELogLevel.Info, string.Format(message, args));
        }

        public void Warning(string message, params object[] args)
        {
            LogImp(ELogLevel.Warning, string.Format(message, args));
        }

        public void Info(string message, params object[] args)
        {
            LogImp(ELogLevel.Info, string.Format(message, args));
        }

        public void Debug(string message, params object[] args)
        {
            LogImp(ELogLevel.Debug, string.Format(message, args));
        }

        public void Error(string message, params object[] args)
        {
            LogImp(ELogLevel.Error, string.Format(message, args));
        }
    }

    public partial class UnityLogger
    {
        private const ELogLevel FilterLevel = ELogLevel.Info;
        private static readonly StringBuilder _stringBuilder = new StringBuilder(1024);

        private enum ELogLevel
        {
            Info,
            Debug,
            Assert,
            Warning,
            Error,
            Exception,
        }

        /// <summary>
        /// 获取日志格式。
        /// </summary>
        /// <param name="eLogLevel">日志级别。</param>
        /// <param name="logString">日志字符。</param>
        /// <param name="bColor">是否使用颜色。</param>
        /// <returns>StringBuilder。</returns>
        private static StringBuilder GetFormatString(ELogLevel eLogLevel, string logString, bool bColor)
        {
            _stringBuilder.Clear();
            switch (eLogLevel)
            {
                case ELogLevel.Debug:
                    _stringBuilder.AppendFormat(
                        bColor
                            ? "<color=#CFCFCF><b>[Debug] ► </b></color> - <color=#00FF18>{0}</color>"
                            : "<color=#00FF18><b>[Debug] ► </b></color> - {0}",
                        logString);
                    break;
                case ELogLevel.Info:
                    _stringBuilder.AppendFormat(
                        bColor
                            ? "<color=#CFCFCF><b>[INFO] ► </b></color> - <color=#CFCFCF>{0}</color>"
                            : "<color=#CFCFCF><b>[INFO] ► </b></color> - {0}",
                        logString);
                    break;
                case ELogLevel.Assert:
                    _stringBuilder.AppendFormat(
                        bColor
                            ? "<color=#FF00BD><b>[ASSERT] ► </b></color> - <color=green>{0}</color>"
                            : "<color=#FF00BD><b>[ASSERT] ► </b></color> - {0}",
                        logString);
                    break;
                case ELogLevel.Warning:
                    _stringBuilder.AppendFormat(
                        bColor
                            ? "<color=#FF9400><b>[WARNING] ► </b></color> - <color=yellow>{0}</color>"
                            : "<color=#FF9400><b>[WARNING] ► </b></color> - {0}",
                        logString);
                    break;
                case ELogLevel.Error:
                    _stringBuilder.AppendFormat(
                        bColor
                            ? "<color=red><b>[ERROR] ► </b></color> - <color=red>{0}</color>"
                            : "<color=red><b>[ERROR] ► </b></color>- {0}",
                        logString);
                    break;
                case ELogLevel.Exception:
                    _stringBuilder.AppendFormat(
                        bColor
                            ? "<color=red><b>[EXCEPTION] ► </b></color> - <color=red>{0}</color>"
                            : "<color=red><b>[EXCEPTION] ► </b></color> - {0}",
                        logString);
                    break;
            }

            return _stringBuilder;
        }

        private static void LogImp(ELogLevel type, string logString)
        {
            if (type < FilterLevel)
            {
                return;
            }
            
            StringBuilder infoBuilder = GetFormatString(type, logString, true);
            string logStr = infoBuilder.ToString();

            //获取C#堆栈,Warning以上级别日志才获取堆栈
            if (type == ELogLevel.Error || type == ELogLevel.Warning || type == ELogLevel.Exception)
            {
                StackFrame[] stackFrames = new StackTrace().GetFrames();
                // ReSharper disable once PossibleNullReferenceException
                for (int i = 0; i < stackFrames.Length; i++)
                {
                    StackFrame frame = stackFrames[i];
                    // ReSharper disable once PossibleNullReferenceException
                    string declaringTypeName = frame.GetMethod().DeclaringType.FullName;
                    string methodName = stackFrames[i].GetMethod().Name;

                    infoBuilder.AppendFormat("[{0}::{1}\n", declaringTypeName, methodName);
                }
                
                logStr = Regex.Replace(logStr, @"(.*ETTaskMethod.*\n)", match =>
                {
                    return "";
                });

                logStr = Regex.Replace(logStr, @"at (.*?) in (.*?\.cs):(\w+)", match =>
                {
                    if (match.Groups[1].Value.Contains("AsyncETTaskMethodBuilder"))
                    {
                        return "";
                    }
                    string path = match.Groups[2].Value;
                    string line = match.Groups[3].Value;
                    // return $"{match.Groups[1].Value}\n<a href=\"{path}\" line=\"{line}\">{path}:{line}</a>";
                    return $"<a href=\"{path}\" line=\"{line}\">{path}:{line}</a>";
                });
            }

            switch (type)
            {
                case ELogLevel.Info:
                case ELogLevel.Debug:
                    UnityEngine.Debug.Log(logStr);
                    break;
                case ELogLevel.Warning:
                    UnityEngine.Debug.LogWarning(logStr);
                    break;
                case ELogLevel.Assert:
                    UnityEngine.Debug.LogAssertion(logStr);
                    break;
                case ELogLevel.Error:
                    UnityEngine.Debug.LogError(logStr);
                    break;
                case ELogLevel.Exception:
                    throw new Exception(logStr);
            }
        }
    }
}