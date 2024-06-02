using System;


namespace HyperEdge.Sdk.Unity.Flexi
{
    public static class Logger
    {
        public static void Info(string message)
        {
            //Console.Log(message);
        }

        public static void Warn(string message)
        {
            //Console.LogWarning(message);
        }

        public static void Error(string message)
        {
            //Console.LogError(message);
        }

        public static void Fatal(Exception e)
        {
            //Console.LogException(e);
        }
    }
}
