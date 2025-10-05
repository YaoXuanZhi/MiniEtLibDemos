using System;
using NLog;
using NLog.Conditions;
using NLog.Config;
using NLog.Targets;

namespace ET
{
    public class NLogger: ILog
    {
        private readonly NLog.Logger logger;

        public NLogger(string name, int process, string configPath)
        {
            LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(configPath);
            LogManager.Configuration.Variables["appIdFormat"] = $"{process:000000}";
            LogManager.Configuration.Variables["currentDir"] = Environment.CurrentDirectory;

            // Add colored console target
            var consoleTarget = new ColoredConsoleTarget("coloredConsole")
            {
                Layout = "${longdate} [${level:uppercase=true}] ${message}",
                UseDefaultRowHighlightingRules = false
            };

            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(
                ConditionParser.ParseExpression("level == LogLevel.Trace"),
                ConsoleOutputColor.DarkGray,
                ConsoleOutputColor.NoChange));
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(
                ConditionParser.ParseExpression("level == LogLevel.Debug"),
                ConsoleOutputColor.Gray,
                ConsoleOutputColor.NoChange));
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(
                ConditionParser.ParseExpression("level == LogLevel.Info"),
                ConsoleOutputColor.White,
                ConsoleOutputColor.NoChange));
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(
                ConditionParser.ParseExpression("level == LogLevel.Warn"),
                ConsoleOutputColor.Yellow,
                ConsoleOutputColor.NoChange));
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(
                ConditionParser.ParseExpression("level == LogLevel.Error"),
                ConsoleOutputColor.Red,
                ConsoleOutputColor.NoChange));
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(
                ConditionParser.ParseExpression("level == LogLevel.Fatal"),
                ConsoleOutputColor.White,
                ConsoleOutputColor.DarkRed));

            LogManager.Configuration.AddTarget(consoleTarget);
            LogManager.Configuration.AddRule(LogLevel.Trace, LogLevel.Fatal, consoleTarget);
            LogManager.ReconfigExistingLoggers();

            this.logger = LogManager.GetLogger(name);
        }

        public void Trace(string message)
        {
            this.logger.Trace(message);
        }

        public void Warning(string message)
        {
            this.logger.Warn(message);
        }

        public void Info(string message)
        {
            this.logger.Info(message);
        }

        public void Debug(string message)
        {
            this.logger.Debug(message);
        }

        public void Error(string message)
        {
            this.logger.Error(message);
        }

        public void Fatal(string message)
        {
            this.logger.Fatal(message);
        }

        public void Trace(string message, params object[] args)
        {
            this.logger.Trace(message, args);
        }

        public void Warning(string message, params object[] args)
        {
            this.logger.Warn(message, args);
        }

        public void Info(string message, params object[] args)
        {
            this.logger.Info(message, args);
        }

        public void Debug(string message, params object[] args)
        {
            this.logger.Debug(message, args);
        }

        public void Error(string message, params object[] args)
        {
            this.logger.Error(message, args);
        }

        public void Fatal(string message, params object[] args)
        {
            this.logger.Fatal(message, args);
        }
    }
}