// See https://aka.ms/new-console-template for more information

using System;
using System.Threading;

namespace ET
{
    public static class Program
    {
        public static void Main()
        {
            Entry.Init();
            Init.Start();

            while (true)
            {
                Thread.Sleep(1);
                try
                {
                    Init.Update();
                    Init.LateUpdate();
                    Init.FrameFinishUpdate();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}