using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace HyperEdge.Sdk.Unity
{
    public class HePyResult
    {
        public HePyResult(Process pyProc)
        {
            ExitCode = pyProc.ExitCode;
            StdOut = pyProc.StandardOutput.ReadToEnd();
            StdErr = pyProc.StandardError.ReadToEnd();
        }
        
        public bool IsSuccess { get => ExitCode == 0; }
        public int ExitCode { get; private set; }
        public string StdOut { get; private set; }
        public string StdErr { get; private set; }
    }
}
