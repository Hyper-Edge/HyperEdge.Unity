using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using MessagePipe;
using Newtonsoft.Json;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEditor;

using HyperEdge.Shared.Protocol.Models;
using HyperEdge.Shared.Protocol.Models.Export;


namespace HyperEdge.Sdk.Unity
{
    public class AppBuilderWindow : EditorWindow
    {
        private const string _CREATE_OR_SELECT_APP_LABEL = "Select or create new App first";

        private enum TabIndices
        {
            APP_TAB_IDX = 0,
            BUILD_TAB_IDX,
            VERSIONS_TAB_IDX,
            APP_ENVIRONMENTS_TAB_IDX,
            SERVERS_TAB_IDX
        };

        private SemaphoreSlim _exportBuildRunSem = new SemaphoreSlim(1, 1);
        private SemaphoreSlim _createNewEnvSem = new SemaphoreSlim(1, 1);

        private int _toolbarIdx = 0;
        private string[] _toolbarStrings = {
            "Application",
            "Build",
            "Versions",
            "Environments",
            "Servers",
        };
        //
        private int _createProjProgress = -1;
        //
        private string _newAppVersionName = "";

        private int _selectedAppEnvIdx = 0;
        private string _appEnvName = "";

        private List<AppData> _appDataList = null;
        private Dictionary<string, AppData> _appDataById = new();
        private int _selectedAppDataIdx = 0;
        private AppData? _appData = null;

        private HyperEdgePy? _py = null;
        private HyperEdgeBackendClient _heClient = null;

        private IDisposable _bag = null;
        private Channel<OnPyExportMessage> _pyExportChannel = Channel.CreateSingleConsumerUnbounded<OnPyExportMessage>();

        private List<Web3AppDTO> _appsList = null;
        private int _selectedAppIdx = 0;
        
        private bool _appsListRequested = false;

        private bool _doCloneApp = false;
        private string _appIdToLoad = string.Empty;
        private string projectName = string.Empty;
        private string projectDescription = string.Empty;
        private string projectVersion = string.Empty;
        private List<HeContainerInfo> _serverList = new List<HeContainerInfo>();
        //
        private List<HeImageInfo> _imagesList = new List<HeImageInfo>();        
        private List<int> _imgEnvSelIdxs = new List<int>();
  
        private Vector2 _scrollPosition;
        private Vector2 _imgListScrollPosition;

        [MenuItem("HyperEdge/AppManager")]
        public static void ShowWindow()
        {
            HyperEdgeWindowManager.ShowWindow<AppBuilderWindow>();
        }

        public void Awake()
        {
            InitCallbacks();
            if (_heClient is null)
            {
                _heClient = new HyperEdgeBackendClient();
            }
            if (_py is null)
            {
                _py = new HyperEdgePy("");
            }
            if (_appData is null)
            {
                SetAppData(AppData.Load());
            }
            LoadLocalAppDataList();
        }

        public void Destroy()
        {
            _bag.Dispose();
        }

        private void LoadLocalAppDataList()
        {
            _appDataList = AssetUtils.FindAssetsByType<AppData>();
            _appDataById.Clear();
            foreach(var appData in _appDataList)
            {
                _appDataById[appData.Id] = appData;
            }
        }

        private void SetAppData(AppData appData)
        {
            _appData = appData;
            if (appData is not null)
            {
                if (_py is null)
                {
                    _py = new HyperEdgePy("");
                }
                if (_heClient is null)
                {
                    _heClient = new HyperEdgeBackendClient();
                }
                _py.ProjectName = appData.Name;
                SerializeAppDataJson();
                SyncAppData(appData);
                AppDataManager.Default.SetAppData(appData);
                AssemblyManager.Instance.AddProject(appData.Name);
                LoadLocalAppDataList();
            }
        }

        private void SyncAppData(AppData appData)
        {
            GetAppEnvironmentsList(appData.Id);
            GetAppVersionsList(appData.Id);
            GetAppServersList(appData.Id).Forget();
            GetAppImagesList(appData.Id).Forget();
        }

        private void InitCallbacks()
        {
            if (_bag is null)
            {
                var d = DisposableBag.CreateBuilder();
                MessageHub.Instance.OnAppsInfo.Subscribe(r =>
                {
                    _appsList = r.Apps;
                    _appsListRequested = false;
                }).AddTo(d);
                //
                MessageHub.Instance.OnAppEnvsInfo.Subscribe(r => OnAppEnvsInfo(r)).AddTo(d);
                MessageHub.Instance.OnPyCollectDone.Subscribe(r => OnPyCollectDone(r)).AddTo(d);
                MessageHub.Instance.OnPyExportDone.Subscribe(r => OnPyExportDone(r)).AddTo(d);
                MessageHub.Instance.OnAppVersionsInfo.Subscribe(r => OnAppVersionsInfo(r)).AddTo(d);
                //
                _bag = d.Build();
            }
        }

        private void OnPyExportDone(OnPyExportMessage resp)
        {
            var appData = ScriptableObject.CreateInstance<AppData>();
            appData.Id = resp.AppId;
            appData.Name = resp.Name;
            SetAppData(CreateNewAppDataAsset(appData));
            _pyExportChannel.Writer.TryWrite(resp);
        }

        private void OnPyCollectDone(OnPyCollectMessage resp)
        {
        }

        private void GetAppsList()
        {
            if (_appsListRequested)
            {
                return;
            }
            if(_heClient != null) {
                _heClient.GetApps().Forget();
            }
            _appsListRequested = true;
        }

        private void GetAppEnvironmentsList(string appId)
        {
            _heClient.GetAppEnvironments(appId).Forget();
        }

        private void GetAppVersionsList(string appId)
        {
            _heClient.GetAppVersions(appId).Forget();
        }

        private void OnGUI()
        {
            _toolbarIdx = GUILayout.Toolbar(_toolbarIdx, _toolbarStrings);
            if (_toolbarIdx == (int)TabIndices.APP_TAB_IDX)
            {
                OnAppInfoTab();
            }
            else if (_toolbarIdx == (int)TabIndices.BUILD_TAB_IDX)
            {
                OnBuildTab();
            }
            else if (_toolbarIdx == (int)TabIndices.VERSIONS_TAB_IDX)
            {
                OnAppVersionsTab();
            }
            else if (_toolbarIdx == (int)TabIndices.APP_ENVIRONMENTS_TAB_IDX)
            {
                OnAppEnvironmentsTab();
            }
            else if (_toolbarIdx == (int)TabIndices.SERVERS_TAB_IDX)
            {
                OnServersTab();
            }
        }

        private void OnAppVersionsTab()
        {
            ReleaseNewAppVersionGUI();
            EditorGUILayout.Space();
            BuildAppVersionGUI();
        }

        private void OnAppEnvironmentsTab()
        {
            if (_appData is null)
            {
                EditorGUILayout.LabelField(_CREATE_OR_SELECT_APP_LABEL);
                return;
            }
            if (_appData.AppEnvironments.Count > 0)
            {
                var appEnvNames = _appData.AppEnvironments.Select(v => $"{v.Name} {v.Id}").ToArray();
                _selectedAppEnvIdx = EditorGUILayout.Popup(_selectedAppEnvIdx, appEnvNames);
                EditorGUILayout.Space();
                var appEnvInfo = _appData.AppEnvironments[_selectedAppEnvIdx];
                EditorGUILayout.LabelField("EnvId:");
                EditorGUILayout.LabelField(appEnvInfo.Id);
            }
            else
            {
                EditorGUILayout.LabelField("Can't find any AppEnvironment defined", EditorStyles.boldLabel);
            }
            EditorGUILayout.Space();
            //
            GUI.enabled = _createNewEnvSem.CurrentCount > 0;
            //
            EditorGUILayout.LabelField("New App Environment:", EditorStyles.boldLabel);
            _appEnvName = EditorGUILayout.TextField(_appEnvName);
            if (GUILayout.Button("Create New App Environment"))
            {
                DoCreateAppEnvironment(_appEnvName);
            }
            //
            GUI.enabled = true;
        }

        private void BuildAppVersionGUI()
        {
            if (_appData is null)
            {
                EditorGUILayout.LabelField(_CREATE_OR_SELECT_APP_LABEL);
                return;
            }
            if (GUILayout.Button("Reload Versions"))
            {
                DownloadAllAppVersionData().Forget();
            }
            GUILayout.Label("App Versions:", EditorStyles.boldLabel, GUILayout.Width(100));
            //
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            //
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Name", EditorStyles.boldLabel, GUILayout.Width(100));
            GUILayout.Label("ImageId", EditorStyles.boldLabel, GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();
            //
            foreach (var appVersionData in _appData.Versions)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(appVersionData.Name, GUILayout.Width(100));
                GUILayout.Label(appVersionData.Images.ServerImageId, GUILayout.Width(200));
                //
                //if (GUILayout.Button("Download SDK", GUILayout.Width(120)))
                //{
                //    DoDownloadSdk(appVersionData).Forget();
                //}
                EditorGUILayout.EndHorizontal();
            }
            //
            EditorGUILayout.EndScrollView();
        }

        private void ReleaseNewAppVersionGUI()
        {
            GUI.enabled = _exportBuildRunSem.CurrentCount > 0;
            //
            EditorGUILayout.LabelField("New Version:");
            _newAppVersionName = EditorGUILayout.TextField(_newAppVersionName);
            if (GUILayout.Button("Release Version"))
            {
                DoReleaseAppVersion(_newAppVersionName).Forget();
            }
            //
            GUI.enabled = true;
        }
        
        private void OnAppInfoTab()
        {
            if (_appDataList is null)
            {
                LoadLocalAppDataList();
            }
            //
            RenderSelectLocalAppDataGUI();
            // GUI for loading AppData by AppId
            RenderCreateNewProjectOrLoadExistingGUI();
            //
            if (_appData is null)
            {
                SetAppData(AppData.Load());
            }
            //
            EditorGUILayout.BeginVertical();
            //
            if (_appData is not null)
            {
                //
                // Display AppData information
                EditorGUILayout.LabelField("Current App:", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("AppId:");
                EditorGUILayout.LabelField(_appData.Id);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Name:");
                EditorGUILayout.LabelField(_appData.Name);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField("Description:");
                EditorGUILayout.LabelField(_appData.Description);
                EditorGUILayout.Space();
            }
            EditorGUILayout.Space();

            /*
            if (GUILayout.Button("Codegen"))
            {
                _py.RunLocalCodeGen().Forget();
            }
            */
            
            EditorGUILayout.EndVertical();
        }

        private void RenderLoadProjectByIdGUI()
        {
            //
            if (_appsList is null)
            {
                GetAppsList();
                return;
            }
            // 
            if (_selectedAppIdx >= _appsList.Count)
            {
                return;
            }
            EditorGUILayout.LabelField("Load Project from HyperEdge", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("App ID to Load:");
            var appNames = _appsList
                .Where(x => !_appDataById.ContainsKey(x.Id.ToString()))
                .Select(x => $"{x.Name} - {x.Id.ToString()}")
                .ToArray();
            _selectedAppIdx = EditorGUILayout.Popup(_selectedAppIdx, appNames);
            if (GUILayout.Button("Load App Data"))
            {
                var selectedAppInfo = _appsList[_selectedAppIdx];
                var appData = ScriptableObject.CreateInstance<AppData>();
                appData.Id = selectedAppInfo.Id.ToString();
                appData.Name = selectedAppInfo.Name;
                SetAppData(CreateNewAppDataAsset(appData));
                // Syncing...
                DownloadAllAppVersionData().Forget();
            }
        }

        private void RenderSelectLocalAppDataGUI()
        {
            EditorGUILayout.LabelField("Local Projects", EditorStyles.boldLabel);
            if (_appDataList.Count == 0)
            {
                GUILayout.Label("Can't find any AppData asset");
                return;
            }
            EditorGUILayout.LabelField("App to Load:");
            var appNames = _appDataList.Select(x => $"{x.Name} - {x.Id}").ToArray();
            _selectedAppDataIdx = EditorGUILayout.Popup(_selectedAppDataIdx, appNames);
            if (GUILayout.Button("Load App Data"))
            {
                SetAppData(_appDataList[_selectedAppDataIdx]);
                DownloadAllAppVersionData().Forget();
            }
        }

        private void RenderCreateNewProjectGUI()
        {
            EditorGUILayout.LabelField("Create New Project", EditorStyles.boldLabel);
            projectName = EditorGUILayout.TextField("Project Name:", projectName);
            projectDescription = EditorGUILayout.TextField("Description:", projectDescription);
            //
            if (GUILayout.Button("Create Project"))
            {
                var py = new HyperEdgePy(projectName);
                CreateAndSaveUnityAsset(py, projectName, projectDescription, "");
            }
        }

        private void RenderExportProjectGUI()
        {
            // Export GUI
            EditorGUILayout.LabelField("Export Existing Project from JSON file", EditorStyles.boldLabel);
            _doCloneApp = EditorGUILayout.Toggle("Clone existing App", _doCloneApp);
            if (GUILayout.Button("Export"))
            {
                OpenAppDataJsonFile();
            }
        }

        private void RenderCreateNewProjectOrLoadExistingGUI()
        {
            EditorGUILayout.BeginVertical();
            //
            RenderLoadProjectByIdGUI();
            EditorGUILayout.Space();
            //
            RenderExportProjectGUI();
            EditorGUILayout.Space();
            //
            RenderCreateNewProjectGUI();
            EditorGUILayout.Space();
            //
            EditorGUILayout.EndVertical();
        }

        private void SerializeAppDataJson()
        {
            var appDataJson = JsonConvert.SerializeObject(_appData);
            File.WriteAllText(_py.GetPythonScriptsPath() + "/app.json", appDataJson);
        }

        private AppData OpenAppDataJsonFile()
        {
            var appJsonPath = EditorUtility.OpenFilePanel("", "Assets", "json");
            if (string.IsNullOrEmpty(appJsonPath))
            {
                return null;
            }
            var appJson = File.ReadAllText(appJsonPath);
            var appData = ScriptableObject.CreateInstance<AppData>();
            JsonConvert.PopulateObject(appJson, appData);
            if (_doCloneApp || string.IsNullOrEmpty(appData.Id))
            {
                DoCloneProject(appData, appJsonPath).Forget();
                return appData;
            }
            else
            {
                return CreateNewAppDataAsset(appData);
            }
        }

        public async UniTaskVoid DoCloneProject(AppData appData, string appDataJsonPath)
        {
            const float CLONE_APP_NSTEPS = 4.0f;
            //
            var projectName = appData.Name;
            var py = new HyperEdgePy(projectName);
            var srcProjectPath = System.IO.Path.GetDirectoryName(appDataJsonPath);
            var tgtProjectPath = py.GetPythonScriptsPath();
            _createProjProgress = UnityEditor.Progress.Start(projectName, $"Cloning project \"{projectName}\"");
            if (srcProjectPath != tgtProjectPath)
            {
                FileUtils.Copy(srcProjectPath, tgtProjectPath);
            }
            //
            appData.Id = "";
            var appDataJson = JsonConvert.SerializeObject(appData);
            File.WriteAllText(py.GetPythonScriptsPath() + "/app.json", appDataJson);
            //
            UnityEditor.Progress.Report(_createProjProgress, 1/CLONE_APP_NSTEPS, $"Copied files");
            //
            var pyRet = await py.Export();
            if (!pyRet.IsSuccess)
            {
                UnityEditor.Progress.Remove(_createProjProgress);
                return;
            } 
            UnityEditor.Progress.Report(_createProjProgress, 2/CLONE_APP_NSTEPS, $"Exported \"{projectName}\"");
            //
            var pyMsg = await _pyExportChannel.Reader.ReadAsync();
            UnityEditor.Progress.Report(_createProjProgress, 3/CLONE_APP_NSTEPS, $"Created AppData asset for \"{projectName}\"");
            //
            UnityEditor.Progress.Remove(_createProjProgress);
        }

        private AppData CreateNewAppDataAsset(AppData appData)
        {
            string folderPath = "Assets/HyperEdge";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets", "HyperEdge");
            }
            
            string assetFileName = GenerateUniqueAssetFileName(folderPath, appData.Name);
            string assetPath = Path.Combine(folderPath, assetFileName);

            AssetDatabase.CreateAsset(appData, assetPath);
            AssetDatabase.Refresh();
            FlushAppData();
            return appData;
        }

        private void OnServersTab()
        {
            if (GUILayout.Button("Refresh"))
            {
                if (_appData != null && _heClient != null) 
                {
                    GetAppServersList(_appData.Id).Forget();
                    GetAppImagesList(_appData.Id).Forget();
                }
                else
                {
                    EditorGUILayout.LabelField("Can't find any AppData asset or HE Client");
                    return;
                }
            }

            GUILayout.Label("Servers:", EditorStyles.boldLabel, GUILayout.Width(100));
            //
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Environment", EditorStyles.boldLabel, GUILayout.Width(100));
            GUILayout.Label("Name", EditorStyles.boldLabel, GUILayout.Width(200));
            GUILayout.Label("Status", EditorStyles.boldLabel, GUILayout.Width(80));
            GUILayout.Label("Action", EditorStyles.boldLabel, GUILayout.Width(60));
            EditorGUILayout.EndHorizontal();

            var runningEnvs = new HashSet<string>();
            foreach (var server in _serverList)
            {
                var envData = _appData.AppEnvironments.Find(v => v.Id == server.EnvId);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(envData is null ? server.EnvId : envData.Name, GUILayout.Width(100));
                GUILayout.Label(server.Name, GUILayout.Width(200));
                GUILayout.Label(server.Status, GUILayout.Width(80));
                bool isRunning = server.Status == "running";
                if (isRunning)
                {
                    runningEnvs.Add(server.EnvId);
                }
                //
                GUI.enabled = _exportBuildRunSem.CurrentCount > 0;
                //
                if (GUILayout.Button(isRunning ? "Stop" : "Run", GUILayout.Width(60)))
                {
                    if (isRunning)
                    {
                        RunStopServer(server, false).Forget();
                    }
                    else
                    {
                        RunStopServer(server, true).Forget();
                    }
                }
                GUI.enabled = true;
                //
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
            //
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            //
            GUILayout.Label("Images:", EditorStyles.boldLabel, GUILayout.Width(100));
            //
            _imgListScrollPosition = EditorGUILayout.BeginScrollView(_imgListScrollPosition);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Version", EditorStyles.boldLabel, GUILayout.Width(200));
            GUILayout.Label("Environment", EditorStyles.boldLabel, GUILayout.Width(180));
            EditorGUILayout.EndHorizontal();
            //
            var appEnvNames = _appData.AppEnvironments
                .Where(v => !runningEnvs.Contains(v.Id))
                .Select(v => v.Name)
                .ToArray();
            //
            if (appEnvNames.Length > 0)
            {
                for (int i = 0; i < _imagesList.Count; i++)
                {
                    var img = _imagesList[i];
                    var verData = _appData.Versions.Find(v => v.Id == img.VersionId);
                    //
                    EditorGUILayout.BeginHorizontal();
                    GUI.enabled = _exportBuildRunSem.CurrentCount > 0;
                    GUILayout.Label($"{img?.HeComponent}-{verData?.Name}", GUILayout.Width(200));
                    _imgEnvSelIdxs[i] = EditorGUILayout.Popup(_imgEnvSelIdxs[i], appEnvNames, GUILayout.Width(180));
                    if (GUILayout.Button("Run", GUILayout.Width(60)))
                    {
                        StartServerContainer(verData.Name, appEnvNames[_imgEnvSelIdxs[i]]).Forget();
                    }
                    GUI.enabled = true;
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private async UniTask<bool> StartServerContainer(string versionName, string envName)
        {
            await _exportBuildRunSem.WaitAsync();
            try
            {
                return await StartServerContainerImpl(versionName, envName);
            }
            finally
            {
                _exportBuildRunSem.Release();
            }
        }

        private async UniTask<bool> StartServerContainerImpl(string versionName, string envName)
        {
            SerializeAppDataJson();
            var pyRet = await _py.RunServer(versionName, envName);
            if (!pyRet.IsSuccess)
            {
                EditorUtility.DisplayDialog("HyperEdge", "Failed to start server", "Ok");
                return false;
            }
            //
            await GetAppServersList(_appData.Id);
            return true;
        }

        private async UniTask<bool> RunStopServer(HeContainerInfo server, bool doRun)
        {
            await _exportBuildRunSem.WaitAsync();
            try
            {
                return await RunStopServerImpl(server, doRun);
            }
            finally
            {
                _exportBuildRunSem.Release();
            }
        }

        private async UniTask<bool> RunStopServerImpl(HeContainerInfo server, bool doRun)
        {
            var verData = _appData.Versions.Find(v => v.Id == server.VersionId);
            var envData = _appData.AppEnvironments.Find(e => e.Id == server.EnvId);
            if (verData is null || envData is null)
            {
                return false;
            }
            if (doRun)
            {
                await _py.RunServer(verData.Name, envData.Name);
            }
            else
            {
                await _py.StopServer(verData.Name, envData.Name);
            }
            //
            await GetAppServersList(_appData.Id);
            return true;
        }

        private void OnBuildTab()
        {
            if (_appData is null)
            {
                EditorGUILayout.LabelField(_CREATE_OR_SELECT_APP_LABEL);
                return;
            }
            RenderManageCurrentVersionGUI();
        }

        private void RenderManageCurrentVersionGUI()
        {
            EditorGUILayout.LabelField("Current App Version", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"App: {_appData.Name}");
            //
            GUI.enabled = _exportBuildRunSem.CurrentCount > 0;
            if (GUILayout.Button("Run Scripts"))
            {
                CollectAppDef().Forget();
            }
            if (GUILayout.Button("Generate SDKs"))
            {
                DoGenCodeCurrentVersion().Forget();
            }
            if (GUILayout.Button("Build Server Mock"))
            {
                DoServerMockBuild().Forget();
            }
            if (GUILayout.Button("Build"))
            {
                DoExportBuildRunCurrentVersion(true, false).Forget();
            }
            if (GUILayout.Button("Build & Run"))
            {
                DoExportBuildRunCurrentVersion(true, true).Forget();
            }
            if (GUILayout.Button("Run"))
            {
                DoExportBuildRunCurrentVersion(false, true).Forget();
            }
            GUI.enabled = true;
        }

        private async UniTask<bool> DoServerMockBuild()
        {
            const float MOCK_BUILD_NSTEPS = 3.0f;
            var projectName = _appData.Name;
            var prog = UnityEditor.Progress.Start(projectName, $"Generating code for server mocks \"{projectName}\"");
            var pyRet = await _py.RunLocalCodeGen();
            if (!pyRet.IsSuccess)
            {
                EditorUtility.DisplayDialog("HyperEdge", "Server Mock code generation failed", "Ok");
                UnityEditor.Progress.Remove(prog);
                return false;
            }
            UnityEditor.Progress.Report(prog, 1/MOCK_BUILD_NSTEPS, "Server Mock code generated");
            //
            AssemblyManager.Instance.Build(projectName);
            var buildSucc = await AssemblyManager.Instance.WaitForBuild(projectName);
            if (!buildSucc)
            {
                EditorUtility.DisplayDialog("HyperEdge", "Server Mock assembly build failed", "Ok");
                UnityEditor.Progress.Remove(prog);
                return false;
            }
            UnityEditor.Progress.Report(prog, 2/MOCK_BUILD_NSTEPS, "ServerMock.dll built");
            //
            UnityEditor.Progress.Remove(prog);
            return true;
        }

        private async UniTask<bool> DoGenCodeCurrentVersion()
        {
            await _exportBuildRunSem.WaitAsync();
            try
            {
                return await DoGenCodeCurrentVersionImpl();
            }
            finally
            {
                _exportBuildRunSem.Release();
            }
        }

        private async UniTask<bool> DoGenCodeCurrentVersionImpl()
        {
            const float GEN_CODE_NSTEPS = 3.0f;
            var projectName = _appData.Name;
            var genCodeProgress = UnityEditor.Progress.Start(projectName, $"Generating SDK's for \"{projectName}\"");
            var pyRet = await _py.ExportGenCodeCurrentVersion();
            if (!pyRet.IsSuccess)
            {
                EditorUtility.DisplayDialog("HyperEdge", "Code generation failed", "Ok");
                UnityEditor.Progress.Remove(genCodeProgress);
                return false;
            }
            UnityEditor.Progress.Report(genCodeProgress, 1/GEN_CODE_NSTEPS, "Generated Code for SDK's");
            //
            var currVersionData = _appData.Versions.Find(v => v.Name == "current");
            if (currVersionData is null)
            {
                EditorUtility.DisplayDialog("HyperEdge", $"Current version doesn't exist", "Ok");
                UnityEditor.Progress.Remove(genCodeProgress);
                return false;
            } 
            var versionInfo = await _heClient.GetAppVersionInfo(currVersionData.Id);
            UnityEditor.Progress.Report(genCodeProgress, 2/GEN_CODE_NSTEPS, "Updated Current version info");
            //
            if (versionInfo.Data.Files is null)
            {
                UnityEditor.Progress.Remove(genCodeProgress);
                return false;
            }
            //
            currVersionData.Files.SdkNodeFlowsFileId = versionInfo.Data.Files.SdkNodeFlowsFileId;
            currVersionData.Files.SdkSharedFileId = versionInfo.Data.Files.SdkSharedFileId;
            currVersionData.Files.SdkServerFileId = versionInfo.Data.Files.SdkServerFileId;
            currVersionData.Files.GameDataFileId = versionInfo.Data.Files.GameDataFileId;
            currVersionData.Files.UnityClientFileId = versionInfo.Data.Files.UnityClientFileId;
            currVersionData.Files.NetPlayFileId = versionInfo.Data.Files.NetPlayFileId;
            currVersionData.Files.TurnBattlerFileId = versionInfo.Data.Files.TurnBattlerFileId;
            EditorUtility.SetDirty(_appData);
            //
            await DoDownloadSdk(currVersionData);
            await DoDownloadGameData(currVersionData);
            UnityEditor.Progress.Report(genCodeProgress, 3/GEN_CODE_NSTEPS, "Downloaded SDK components");
            //
            UnityEditor.Progress.Remove(genCodeProgress);
            return true;
        }

        private bool IsSdkMissing()
        {
            var sdkComponents = new string[] {"Sdk.Shared", "Sdk.Server", "Sdk.NodeFlows", "UnityClient", "NetPlay"};
            foreach (var component in sdkComponents)
            {
                var tgtDirname = $"Assets/Scripts/{_appData.Name}.{component}";
                if (!Directory.Exists(tgtDirname))
                {
                    return true;
                }
            }
            return false;
        }

        private async UniTask<bool> DoExportBuildRunCurrentVersion(bool doBuild, bool doRun)
        {
            await _exportBuildRunSem.WaitAsync();
            try
            {
                return await DoExportBuildRunCurrentVersionImpl(doBuild, doRun);
            }
            finally
            {
                _exportBuildRunSem.Release();
            }
        }

        private async UniTask<bool> DoExportBuildRunCurrentVersionImpl(bool doBuild, bool doRun)
        {
            string actionDesc = "Export";
            if (doBuild)
            {
                actionDesc += "&Build";
            }
            if (doRun)
            {
                actionDesc += "&Run";
            }

            const float EXPORT_BUILD_RUN_NSTEPS = 3.0f;
            var projectName = _appData.Name;

            var prog = UnityEditor.Progress.Start(projectName, $"{actionDesc} \"{projectName}\"");
            var currVersionData = _appData.Versions.Find(v => v.Name == "current");
            if (currVersionData is null)
            {
                EditorUtility.DisplayDialog("HyperEdge", $"Current version doesn't exist", "Ok");
                UnityEditor.Progress.Remove(prog);
                return false;
            }
            if (currVersionData.Files.Empty || IsSdkMissing())
            {
                EditorUtility.DisplayDialog("HyperEdge", $"Code generation started", "Ok");
                var genCodeSucc = await DoGenCodeCurrentVersionImpl();
                if (!genCodeSucc)
                {
                    return false;
                }
            }
            var succ = await DoServerMockBuild();
            if (!succ)
            {
                EditorUtility.DisplayDialog("HyperEdge", "Server Mock code generation failed", "Ok");
                UnityEditor.Progress.Remove(prog);
                return false;
            }
            UnityEditor.Progress.Report(prog, 1/EXPORT_BUILD_RUN_NSTEPS, "Server Mock code generated");
            //
            var pyRet = await _py.ExportBuildRunCurrentVersion(doBuild, doRun);
            UnityEditor.Progress.Report(prog, 2/EXPORT_BUILD_RUN_NSTEPS, $"{actionDesc} current version");
            if (!pyRet.IsSuccess)
            {
                EditorUtility.DisplayDialog("HyperEdge", $"{actionDesc} Failed", "Ok");
                UnityEditor.Progress.Remove(prog);
                return false;
            }
            //
            UnityEditor.Progress.Remove(prog);
            EditorUtility.DisplayDialog("HyperEdge", $"{actionDesc} Success", "Ok");
            return true;
        }

        private async UniTask<bool> DoReleaseAppVersion(string versionName)
        {
            await _exportBuildRunSem.WaitAsync();
            try
            {
                return await DoReleaseAppVersionImpl(versionName);
            }
            finally
            {
                _exportBuildRunSem.Release();
            }
        }

        private async UniTask<bool> DoReleaseAppVersionImpl(string versionName)
        {
            SerializeAppDataJson();
            var pyRet = await _py.ReleaseAppVersion(versionName);
            if (!pyRet.IsSuccess)
            {
                EditorUtility.DisplayDialog("HyperEdge", $"Release {versionName} Failed", "Ok");
                return false;
            }
            //
            await DownloadAllAppVersionData();
            //
            EditorUtility.DisplayDialog("HyperEdge", $"Release Success", "Ok");
            return true;
        }

        private async UniTask DoDownloadGameData(AppVersionInfo versionInfo)
        {
            var gameDataPath = $"Assets/HyperEdge/{_appData.Name}/AppVersions/GameData-{versionInfo.Name}.bin";
            await _heClient.DownloadFileById(versionInfo.Files.GameDataFileId, gameDataPath);
        }

        private async UniTask DoDownloadSdk(AppVersionInfo appVersionData)
        {
            var sdkComponents = new (string, string)[] {
                ( appVersionData.Files.SdkSharedFileId, "Sdk.Shared" ), 
                ( appVersionData.Files.SdkServerFileId, "Sdk.Server" ),
                ( appVersionData.Files.SdkNodeFlowsFileId, "Sdk.NodeFlows" ),
                ( appVersionData.Files.UnityClientFileId, "UnityClient" ),
                ( appVersionData.Files.TurnBattlerFileId, "TurnBattler" ),
                ( appVersionData.Files.NetPlayFileId, "NetPlay" ),
            };
            //
            for (int i = 0; i < sdkComponents.Length; i++)
            {
                if (string.IsNullOrEmpty(sdkComponents[i].Item1))
                {
                    continue;
                }
                await DownloadSdkComponent(sdkComponents[i].Item1, sdkComponents[i].Item2);    
                if (EditorUtility.DisplayCancelableProgressBar(
                        "SDK Download",
                        $"Downloading {sdkComponents[i].Item2} for version {appVersionData.Name}",
                        (float)(i+1)/sdkComponents.Length))
                {
                    break;
                }
            }
            //
            EditorUtility.ClearProgressBar();
        }

        private async UniTask DownloadSdkComponent(string fileId, string component)
        {
            var tempFilename = $"Temp/HyperEdge/{component}.zip";
            var tgtDirname = $"Assets/Scripts/{_appData.Name}.{component}";
            await _heClient.DownloadFileById(fileId, tempFilename);
            if (Directory.Exists(tgtDirname))
            {
                Directory.Delete(tgtDirname, true);
            }
            ZipFile.ExtractToDirectory(tempFilename, tgtDirname);
        }

        private void DoCreateAppEnvironment(string appEnvName)
        {
            CreateNewAppEnvironment(appEnvName).Forget();
        }

        private async UniTask<bool> CreateNewAppEnvironment(string appEnvName)
        {
            await _createNewEnvSem.WaitAsync();
            try
            {
                return await CreateNewAppEnvironmentImpl(appEnvName);
            }
            finally
            {
                _createNewEnvSem.Release();
            }
        }

        private async UniTask<bool> CreateNewAppEnvironmentImpl(string appEnvName)
        {
            var appEnvData = await _heClient.CreateAppEnvironment(_appData.Id, appEnvName);
            var appEnvInfo = new AppEnvironmentInfo {
                Id = appEnvData.Id.ToString(),
                Name = appEnvData.Name
            };
            _appData.AppEnvironments.Add(appEnvInfo);
            FlushAppData();
            return true;
        }

        private void OnAppEnvsInfo(GetAppEnvsResponse r)
        {
            if (r.Envs.Count == 0)
            {
                return;
            }
            var appId = r.Envs[0].AppId;
            if (!_appDataById.ContainsKey(appId))
            {
                return;
            }
            var appData = _appDataById[appId];
            appData.AppEnvironments.Clear();
            foreach (var appEnvData in r.Envs)
            {
                var appEnvInfo = new AppEnvironmentInfo {
                    Id = appEnvData.Id.ToString(),
                    Name = appEnvData.Name
                };
                appData.AppEnvironments.Add(appEnvInfo);
            }
            EditorUtility.SetDirty(appData);
        }

        private void OnAppVersionsInfo(GetAppVersionsResponse r)
        {
            if (r.Versions.Count == 0)
            {
                return;
            }
            var appId = r.Versions[0].AppId;
            if (!_appDataById.ContainsKey(appId))
            {
                return;
            }
            var appData = _appDataById[appId];
            appData.Versions.Clear();
            for (int i = 0; i < r.Versions.Count; i++)
            {
                var versionInfo = r.Versions[i];
                var appDataVersion = new AppVersionInfo {
                    Id = versionInfo.Id,
                    Name = versionInfo.Name
                };
                //
                if (versionInfo.Data.Files is not null)
                {
                    appDataVersion.Files.SdkNodeFlowsFileId = versionInfo.Data.Files.SdkNodeFlowsFileId;
                    appDataVersion.Files.SdkSharedFileId = versionInfo.Data.Files.SdkSharedFileId;
                    appDataVersion.Files.SdkServerFileId = versionInfo.Data.Files.SdkServerFileId;
                    appDataVersion.Files.GameDataFileId = versionInfo.Data.Files.GameDataFileId;
                    appDataVersion.Files.UnityClientFileId = versionInfo.Data.Files.UnityClientFileId;
                    appDataVersion.Files.TurnBattlerFileId = versionInfo.Data.Files.TurnBattlerFileId;
                    appDataVersion.Files.NetPlayFileId = versionInfo.Data.Files.NetPlayFileId;
                }
                //
                if (versionInfo.Data.Images is not null)
                {
                    appDataVersion.Images.ServerImageId = versionInfo.Data.Images.ServerImageId;
                    appDataVersion.Images.SyncBotImageId = versionInfo.Data.Images.SyncBotImageId;
                }
                appData.Versions.Add(appDataVersion);
                //
                DownloadVersionAppDefFile(appData, versionInfo).Forget();
            }
            EditorUtility.SetDirty(appData);
        }

        private async UniTask DownloadVersionAppDefFile(AppData appData, AppVersionInfoDTO versionInfo)
        {
            var versionAppDefPath = $"Assets/HyperEdge/{appData.Name}/AppVersions/{versionInfo.Name}.json";
            if (File.Exists(versionAppDefPath))
            {
                return;
            }
            var downloadProg = UnityEditor.Progress.Start(projectName, $"Downloading {versionInfo.Name}.json");
            var fileLink = await _heClient.GetFileLink(versionInfo.Data.AppDefFileId);
            UnityEditor.Progress.Report(downloadProg, 1/2.0f, $"Fetched file link for {versionInfo.Name}.json");
            await _heClient.DownloadFile(fileLink, versionAppDefPath);
            UnityEditor.Progress.Report(downloadProg, 2/2.0f, $"Downloaded {versionInfo.Name}.json");
            UnityEditor.Progress.Remove(downloadProg);
        }

        private async UniTask DownloadAllAppVersionData()
        {
            var versions = await _heClient.GetAppVersions(_appData.Id);
            _appData.Versions.Clear();
            for (int i = 0; i < versions.Count; i++)
            {
                var versionInfo = versions[i];
                var appDataVersion = new AppVersionInfo {
                    Id = versionInfo.Id,
                    Name = versionInfo.Name
                };
                _appData.Versions.Add(appDataVersion);
                //
                if (versionInfo.Data.Files is not null)
                {
                    appDataVersion.Files.SdkNodeFlowsFileId = versionInfo.Data.Files.SdkNodeFlowsFileId;
                    appDataVersion.Files.SdkSharedFileId = versionInfo.Data.Files.SdkSharedFileId;
                    appDataVersion.Files.SdkServerFileId = versionInfo.Data.Files.SdkServerFileId;
                    appDataVersion.Files.GameDataFileId = versionInfo.Data.Files.GameDataFileId;
                    appDataVersion.Files.UnityClientFileId = versionInfo.Data.Files.UnityClientFileId;
                    appDataVersion.Files.TurnBattlerFileId = versionInfo.Data.Files.TurnBattlerFileId;
                    appDataVersion.Files.NetPlayFileId = versionInfo.Data.Files.NetPlayFileId;
                }
                //
                if (versionInfo.Data.Images is not null)
                {
                    appDataVersion.Images.ServerImageId = versionInfo.Data.Images.ServerImageId;
                    appDataVersion.Images.SyncBotImageId = versionInfo.Data.Images.SyncBotImageId;
                }
                //
                await DownloadVersionAppDefFile(_appData, versionInfo);
                if (EditorUtility.DisplayCancelableProgressBar(
                        "Sync",
                        $"Downloading AppDef file for verions {versionInfo.Name}",
                        (float)(i+1)/versions.Count))
                {
                    break;
                }
            }
            //
            var filesList = await _heClient.GetAppFiles(_appData.Id);
            _appData.Files = new();
            //
            EditorUtility.SetDirty(_appData);
            AssetDatabase.SaveAssets();
            //
            EditorUtility.ClearProgressBar();
        }
        
	    private async UniTask GetAppServersList(string appId)
	    {            
            _serverList = await _heClient.GetAppContainers(appId);
            //Repaint();
	    }

        private async UniTask GetAppImagesList(string appId)
        {
            _imagesList = await _heClient.GetAppImages(appId);
            _imgEnvSelIdxs = _imagesList.Select(v => 0).ToList();
           //Repaint();
        }

        private void FlushAppData()
        {
            EditorUtility.SetDirty(_appData);
            AssetDatabase.SaveAssets();
            _appDataList = null;
        }


        private async UniTask<bool> CollectAppDef()
        {
            await _exportBuildRunSem.WaitAsync();
            try
            {
                return await CollectAppDefImpl();
            }
            finally
            {
                _exportBuildRunSem.Release();
            }
            Repaint();
        }
        
        private async UniTask<bool> CollectAppDefImpl()
        {
            var pyRet = await _py.Collect();
            if (!pyRet.IsSuccess)
            {
                EditorUtility.DisplayDialog("HyperEdge", "Running python scripts failed", "Ok");
                return false;
            }

            EditorUtility.DisplayDialog("HyperEdge", "Success", "Ok");
            return true;
        }

        private async UniTask<HePyResult> DoExportApp(string projectName)
        {
            string projectSlug = projectName.ToLower().Replace(" ", "_").Replace("-", "_");
            var py = new HyperEdgePy(projectSlug);
            return await py.Export();
        }
        
        private string GenerateUniqueAssetFileName(string folderPath, string baseName)
        {
            string fileName = string.IsNullOrEmpty(baseName) ? "AppData" : baseName;
            string assetPath = Path.Combine(folderPath, $"{fileName}.asset");
            int counter = 1;

            // If the file exists, append a number to make the file name unique
            while (AssetDatabase.LoadAssetAtPath<AppData>(assetPath) != null)
            {
                assetPath = Path.Combine(folderPath, $"{fileName}_{counter}.asset");
                counter++;
            }

            return Path.GetFileName(assetPath);
        }
        
        public async UniTaskVoid CreateAndSaveUnityAsset(HyperEdgePy py, string projectName, string projectDescription, string version)
        {
            const float EXPORT_NEW_APP_NSTEPS = 4.0f;
            _createProjProgress = UnityEditor.Progress.Start(projectName, $"Create new project \"{projectName}\"");
            var pyRet = await py.CreateProject(projectName, projectDescription, version);
            if (!pyRet.IsSuccess)
            {
                UnityEditor.Progress.Remove(_createProjProgress);
                EditorUtility.DisplayDialog("HyperEdge", "Scaffolding new project failed", "Ok");
                return;
            } 
            UnityEditor.Progress.Report(_createProjProgress, 1/EXPORT_NEW_APP_NSTEPS, $"Scaffolded project \"{projectName}\"");
            //
            pyRet = await DoExportApp(projectName);
            if (!pyRet.IsSuccess)
            {
                UnityEditor.Progress.Remove(_createProjProgress);
                EditorUtility.DisplayDialog("HyperEdge", "Exporting new project failed", "Ok");
                return;
            } 
            UnityEditor.Progress.Report(_createProjProgress, 2/EXPORT_NEW_APP_NSTEPS, $"Exported \"{projectName}\"");
            //
            var appDataJson = File.ReadAllText(py.GetPythonScriptsPath() + "/app.json");
            var appData = ScriptableObject.CreateInstance<AppData>();
            JsonConvert.PopulateObject(appDataJson, appData);
            SetAppData(CreateNewAppDataAsset(appData));
            UnityEditor.Progress.Report(_createProjProgress, 3/EXPORT_NEW_APP_NSTEPS, $"Created AppData asset for \"{projectName}\"");
            //
            UnityEditor.Progress.Remove(_createProjProgress);
            EditorUtility.DisplayDialog("HyperEdge", $"Successfully created new project \"{projectName}\"", "Ok");
        }
    }
}

