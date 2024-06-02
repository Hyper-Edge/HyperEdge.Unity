using System;
using System.Collections.Generic;
using UnityEditor;


namespace HyperEdge.Sdk.Unity
{
    public static class CodeEditorServer
    {
        private static System.Diagnostics.Process _pyWebEditor = null;
        private static object _lock = new object();

        public static void Start()
        {
            lock(_lock)
            {
                if (_pyWebEditor is null)
                {
                    _pyWebEditor = HyperEdgePy.StartWebEditor();
                }
            }
        }

        [MenuItem("HyperEdge/CodeEditor/Restart")]
        public static void Restart()
        {
            Stop();
            Start();
        }

        public static void Stop()
        {
            lock(_lock)
            {
                if (_pyWebEditor is not null && !_pyWebEditor.HasExited)
                {
                    _pyWebEditor.CloseMainWindow();
                    _pyWebEditor.WaitForExit();
                    _pyWebEditor = null;
                }
            }
        }
    }
}

