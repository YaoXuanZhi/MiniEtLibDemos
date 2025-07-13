using System;
using ET;
using System.Text.RegularExpressions;

namespace TestCase
{
    public class NUnitLogger: ILog
    {
        public void Trace(string msg)
        {
            NUnit.Framework.TestContext.WriteLine(msg);
        }

        public void Debug(string msg)
        {
            NUnit.Framework.TestContext.WriteLine(msg);
        }

        public void Info(string msg)
        {
            NUnit.Framework.TestContext.WriteLine(msg);
        }

        public void Warning(string msg)
        {
            NUnit.Framework.TestContext.WriteLine(msg);
        }

        public void Error(string msg)
        {
#if UNITY_EDITOR || NETCOREAPP
            msg = Msg2LinkStackMsg(msg);
#endif
            NUnit.Framework.TestContext.WriteLine(msg);
        }

        private static string Msg2LinkStackMsg(string msg)
        {
            msg = Regex.Replace(msg, @"(.*ETTaskMethod.*\n)", match =>
            {
                return "";
            });

            return msg;
        }

        public void Error(Exception e)
        {
            NUnit.Framework.TestContext.WriteLine(e);
        }

        public void Trace(string message, params object[] args)
        {
            NUnit.Framework.TestContext.WriteLine(message, args);
        }

        public void Warning(string message, params object[] args)
        {
            NUnit.Framework.TestContext.WriteLine(message, args);
        }

        public void Info(string message, params object[] args)
        {
            NUnit.Framework.TestContext.WriteLine(message, args);
        }

        public void Debug(string message, params object[] args)
        {
            NUnit.Framework.TestContext.WriteLine(message, args);
        }

        public void Error(string message, params object[] args)
        {
            NUnit.Framework.TestContext.WriteLine(message, args);
        }
    }
}