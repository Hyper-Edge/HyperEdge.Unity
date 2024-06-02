using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;


namespace HyperEdge.Sdk.Unity
{
    public class HeProjectAssembly
    {
        public bool BuildRequested { get; set; } = false;
        public Channel<bool> Channel { get; set; }
    }

    public class AssemblyManager : IDisposable
    {
        private static AssemblyManager _instance = new();
        public static AssemblyManager Instance { get => _instance; }

        private Dictionary<string, HeProjectAssembly> _projects = new();

        public AssemblyManager()
        {
            //CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;
            //CompilationPipeline.assemblyCompilationNotRequired += OnAssemblyCompilationNotRequired;
            //CompilationPipeline.assemblyCompilationStarted += OnAssemblyCompilationStarted;
        }

        public void Dispose()
        {
            //CompilationPipeline.assemblyCompilationFinished -= OnAssemblyCompilationFinished;
            //CompilationPipeline.assemblyCompilationNotRequired -= OnAssemblyCompilationNotRequired;
            //CompilationPipeline.assemblyCompilationStarted -= OnAssemblyCompilationStarted;
        }

        public bool AddProject(string projName)
        {
            if (_projects.ContainsKey(projName))
            {
                return false;
            }
            var heProjAssembly = new HeProjectAssembly
            {
                Channel = Channel.CreateSingleConsumerUnbounded<bool>()
            };
            _projects[projName] = heProjAssembly;
            return true;
        }

        public async UniTask<bool> WaitForBuild(string projectName)
        {
            var heAsm = _projects[projectName];
            return await heAsm.Channel.Reader.ReadAsync();
        }

        public Channel<bool>? GetProjectChannel(string projName)
        {
            if (!_projects.TryGetValue(projName, out var heProjAsm))
            {
                return null;
            }
            return heProjAsm.Channel;
        }

        public void Build(string projectName)
        {
            var heAsm = _projects[projectName];
            heAsm.BuildRequested = true;

            var srcDir = $"Assets/HyperEdge/{projectName}/ServerAssemblies";
            var srcFiles = Directory.GetFiles(srcDir, "*.cs", SearchOption.AllDirectories);
            var dllName = $"HyperEdge.App.{projectName}.ServerMock.dll";
            var dllOutPath = $"Temp/HyperEdge/{projectName}/Assemblies/{dllName}";
            //
            var asmBuilder = new AssemblyBuilder(dllOutPath, srcFiles);
            // 
            asmBuilder.buildStarted += OnAssemblyCompilationStarted;
            asmBuilder.buildFinished += OnAssemblyCompilationFinished;
            asmBuilder.Build();
        }

        public void OnAssemblyCompilationStarted(string assemblyPath)
        {
        }

        public void OnAssemblyCompilationFinished(string assemblyPath, CompilerMessage[] messages)
        {
            var asmFileName = Path.GetFileName(assemblyPath);
            Debug.Log($"Compilation finished: {asmFileName}");
            var words = asmFileName.Split('.');
            int numErrors = messages.Count(m => m.type == CompilerMessageType.Error);
            int numWarnings = messages.Count(m => m.type == CompilerMessageType.Warning);
            if (words.Length >= 5 && words[0] == "HyperEdge" && words[words.Length - 2] == "ServerMock")
            {
                //
                if (_projects.TryGetValue(words[2], out var heAsm) && heAsm.BuildRequested)
                {
                    Debug.Log($"{asmFileName} has {numErrors} compilation errors and {numWarnings} warnings");
                    heAsm.BuildRequested = false;
                    heAsm.Channel.Writer.TryWrite(numErrors == 0);
                }
            }
        }
    }
}

