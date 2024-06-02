using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEditor.Scripting.Python;
using UnityEngine;


namespace HyperEdge.Sdk.Unity
{
    public class HyperEdgePy
    {
        static string HePyPath = System.IO.Path.GetFullPath("Packages/tech.hyperedgelabs.unity-plugin/Editor/HyperEdge/Shared/Python/pysources");
        static string HeCliPyPath = HePyPath + "/he-cli.py";
        static string HeWebEditorPath = HePyPath + "/run_dev.py";
        static string UserPythonScriptsPath = "/Assets/PythonScripts~";

        public string ProjectName { get; set; } = string.Empty;
        public string ProjectSlug { get => ProjectName.ToLower().Replace(" ", "_").Replace("-", "_"); }
        public string HeAdminPyPath { get => $"{UserPythonScriptsPath}/{ProjectName}/he-admin.py"; }

        public HyperEdgePy(string projectName)
        {
            this.ProjectName = projectName;
        }

        public static string GetPythonScriptsPath(string prjName)
        {
            var currentDirectory = System.IO.Directory.GetCurrentDirectory();
            return currentDirectory + UserPythonScriptsPath + "/" + prjName;
        }

        public string GetPythonScriptsPath()
        {
            var currentDirectory = System.IO.Directory.GetCurrentDirectory();
            return currentDirectory + UserPythonScriptsPath + "/" + ProjectSlug;
        }

        public async UniTask<HePyResult> Collect()
        {
            return await RunHeAdminPy(new string[] { "collect" });
        }

        public async UniTask<HePyResult> RunLocalCodeGen()
        {
            return await RunHeAdminPy(new string[] { "codegen" });
        }

        public async UniTask<HePyResult> GenerateClientCode()
        {
            return await RunHeAdminPy(new string[] { "generate-client-code" });
        }

        public async UniTask<HePyResult> ExportGenCodeCurrentVersion()
        {
            return await RunHeAdminPy(new string[] { "export-gen-code-current-version" });
        }

        public async UniTask<HePyResult> ExportBuildRunCurrentVersion(bool doBuild, bool doRun)
        {
            return await RunHeAdminPy(new string[] { "export-build-run-current-version", doBuild.ToString(), doRun.ToString() });
        }

        public async UniTask<HePyResult> Export()
        {
            return await RunHeAdminPy(new string[] { "export" });
        }

        public async UniTask<HePyResult> BuildAppVersion(string versionName)
        {
            return await RunHeAdminPy(new string[] { "build-app-version", versionName });
        }

        public async UniTask<HePyResult> GenCodeAppVersion(string versionName)
        {
            return await RunHeAdminPy(new string[] { "gen-code-app-version", versionName });
        }

        public async UniTask<HePyResult> ReleaseAppVersion(string versionName)
        {
            return await RunHeAdminPy(new string[] { "release", versionName });
        }

        public async UniTask<HePyResult> RunServer(string versionName, string envName)
        {
            return await RunHeAdminPy(new string[] { "run", versionName, envName });
        }

        public async UniTask<HePyResult> StopServer(string versionName, string envName)
        {
            return await RunHeAdminPy(new string[] { "stop", versionName, envName });
        }

        public async UniTask<HePyResult> CreateDataClass(string name, string flds)
        {
            var args = new string[] { "create-dataclass", name,
                "--fields", flds };
            return await RunHeAdminPy(args);
        }

        public async UniTask<HePyResult> CreateModelClass(string name, string dataFlds, string modelFlds)
        {
            var args = new string[] { "create-model", name,
                "--data-fields", dataFlds,
                "--model-fields", modelFlds };
            return await RunHeAdminPy(args);
        }

        public async UniTask<HePyResult> CreateStruct(string name, string flds)
        {
            var args = new string[] { "create-struct", name,
                "--fields", flds };
            return await RunHeAdminPy(args);
        }

        public async UniTask<HePyResult> CreateEventClass(string name, string flds)
        {
            var args = new string[] { "create-event", name,
                "--fields", flds };
            return await RunHeAdminPy(args);
        }

        public async UniTask<HePyResult> CreateStorageClass(string name, string storageType, string flds)
        {
            var args = new string[] { "create-storage", name,
                "--storage-type", storageType,
                "--fields", flds };
            return await RunHeAdminPy(args);
        }

        public async UniTask<HePyResult> CreateUserGroup(string name, string flds)
        {
            var args = new string[] { "create-user-group", name,
                "--fields", flds };
            return await RunHeAdminPy(args);
        }

        public async UniTask<HePyResult> CreateLadder(string name, bool isExpBased, string levelDataFields)
        {
            var args = new string[] {
                "create-ladder",
                "--level-data-fields", levelDataFields,
                "--is-exp-based",
                name
            };
            return await RunHeAdminPy(args);
        }
        public async UniTask<HePyResult> CreateEnergySystem(string name, string dataFlds, string modelFlds)
        {
            var args = new string[] { "create-energy-system", name,
                "--data-fields", dataFlds,
                "--model-fields", modelFlds };
            return await RunHeAdminPy(args);
        }

        public async UniTask<HePyResult> CreateBattlePass(string name, string dataFlds, string levelDataFields, string modelFlds)
        {
            var args = new string[] { "create-battlepass", name,
                "--data-fields", dataFlds,
                "--level-data-fields", levelDataFields,
                "--model-fields", modelFlds };
            return await RunHeAdminPy(args);
        }

        public async UniTask<HePyResult> CreateTournament(string name, string dataFlds, string modelFlds)
        {
            var args = new string[] { "create-tournament", name,
                "--data-fields", dataFlds,
                "--model-fields", modelFlds };
            return await RunHeAdminPy(args);
        }

        public async UniTask<HePyResult> CreateRequestHandler(string name, string reqFlds, string respFlds)
        {
            var args = new string[] { "create-req-handler", name,
                "--req-fields", reqFlds,
                "--resp-fields", respFlds };
            return await RunHeAdminPy(args);
        }

        public async UniTask<HePyResult> CreateJobHandler(string name, string reqFlds)
        {
            var args = new string[] { "create-job-handler", name,
                "--job-data-fields", reqFlds
            };
            return await RunHeAdminPy(args);
        }

        public async UniTask<HePyResult> LlmGenData(string dataClsName)
        {
            var args = new string[] { "llm-gen-data", dataClsName };
            return await RunHeAdminPy(args);
        }

        public async UniTask<HePyResult> LlmProposeModel(string name, string description)
        {
            var args = new string[] { "llm-propose-model", name, description };
            return await RunHeAdminPy(args);
        }

        public async UniTask<HePyResult> ConvertLlmThreadToAppDef(string llmThreadId)
        {
            var args = new string[] { "convert-llm-thread-to-app-def", llmThreadId };
            return await RunHeAdminPy(args);
        }

        public async UniTask<HePyResult> RunHeAdminPy(IEnumerable<string> argsToHeAdmin)
        {
            var currentDirectory = System.IO.Directory.GetCurrentDirectory();
            var heAdminPyPath = "\"" + currentDirectory + HeAdminPyPath + "\"";
            var args = new List<string> { heAdminPyPath }.Concat(argsToHeAdmin);

            string argsString = string.Join(" ", args);
            Debug.Log("Running Python script with arguments: " + argsString);
            //
            using (var pyProc = PythonRunner.SpawnPythonProcess(
                    args,
                    environment: new Dictionary<string, string> {
                        {"UNIPIPE", UniPipeServer.DefaultAddress},
                        {"HE_SYS_PATH", HePyPath },
                        {"HE_API_KEY", AppBuilderSettings.XApiKey},
                        {"HE_API_URL", HyperEdgeConstants.BackendUrl},
                        {"HE_CERT_PATH", System.IO.Path.GetFullPath("Packages/tech.hyperedgelabs.unity-plugin/Settings/cert.pem")},
                        {"HE_ASSETS_PATH", currentDirectory + "/Assets/HyperEdge/" + ProjectName},
                        {"HE_APP_PATH", GetPythonScriptsPath()},
                        {"HE_APP_JSON_PATH", GetPythonScriptsPath() + "/app.json"}
                    },
                    redirectOutput: true))
            {
                while(!pyProc.HasExited)
                {
                    await UniTask.Delay(1000);
                }
                var ret = new HePyResult(pyProc);
                Debug.Log($"STDOUT:\n {ret.StdOut}");
                Debug.Log($"STDERR:\n {ret.StdErr}");
                return ret;
            }
        }
        
        public async UniTask<HePyResult> CreateProject(string projectName, string projectDescription = null, string version = null)
        {  
            var currentDirectory = System.IO.Directory.GetCurrentDirectory();

            // Generate the project slug from the project name
            string projectSlug = projectName.ToLower().Replace(" ", "_").Replace("-", "_");

            // Build the argument list for the Python script
            var args = new List<string> { HeCliPyPath, /*"create_project",*/
                "--directory", HePyPath + "/he-sdk-template~",
                "--output-dir", "\"" + currentDirectory + UserPythonScriptsPath + "\"",
                "--extra-context"
            };

            string formattedString = $"\"{{\\\"project_name\\\": \\\"{projectName}\\\", \\\"project_slug\\\": \\\"{projectSlug}\\\", \\\"project_short_description\\\": \\\"{projectDescription}\\\", \\\"version\\\": \\\"{version}\\\"}}\"";
            args.Add($"{formattedString}");
            
            return await RunPythonScript(args);

        }

        public static System.Diagnostics.Process StartWebEditor()
        {
            //
            var currentDirectory = System.IO.Directory.GetCurrentDirectory();
            var args = new List<string> { "\"" + HeWebEditorPath + "\"",
                "\"" + currentDirectory + "/Assets/PythonScripts~/\""
            }; 
            //
            return PythonRunner.SpawnPythonProcess(args, redirectOutput: true,
                environment: new Dictionary<string, string> {
                    {"UNIPIPE", UniPipeServer.DefaultAddress},
                    {"HE_SYS_PATH", HePyPath },
            });
        }

        private static async UniTask<HePyResult> RunPythonScript(IEnumerable<string> args)
        {
            // PythonRunner.SpawnPythonProcess expects a list of strings as arguments
            string argsString = string.Join(" ", args);
            Debug.Log("Running Python script with arguments: " + argsString);

            var currentDirectory = System.IO.Directory.GetCurrentDirectory();
            // Assuming PythonRunner.SpawnPythonProcess can handle output redirection
            var pyProc = PythonRunner.SpawnPythonProcess(args, redirectOutput: true,
                environment: new Dictionary<string, string> {
                    {"UNIPIPE", UniPipeServer.DefaultAddress},
                    {"HE_SYS_PATH", HePyPath },
            });
            if (pyProc != null)
            {
                await UniTask.WaitUntil(() => pyProc.HasExited);
                var ret = new HePyResult(pyProc);
                if (ret.ExitCode != 0)
                {
                    Debug.LogError($"Python script exited with error code {ret.ExitCode}.\n{ret.StdErr}");
                }
                return ret;
            }
            else
            {
                Debug.LogError("Failed to start Python process.");
                return null;
            }
        }
    }
}
