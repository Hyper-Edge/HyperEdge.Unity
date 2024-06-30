using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MessagePipe;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEditor;


namespace HyperEdge.Sdk.Unity.APITester
{
    public class APITesterWindow : EditorWindow
    {
        private AppData? _appData = null;
        private AppDef? _appDef = null;
        private HyperEdgeBackendClient _heClient = null;
        private Dictionary<string, ServerClient> _serverClients = new();
        private Dictionary<string, LocalAccountRepo> _accountRepos = new();

        private IDisposable _bag = null;
        private List<ServerInfo> _serverList = new();
        private int _selectedServerIdx = 0;
        private bool _isServerHealthy = false;

        private CancellationTokenSource _cts = new();

        private int _selectedAppVersionIdx = -1;
	    private int _selectedReqHandlerIdx = 0;
	    private int _selectedJobHandlerIdx = 0;
	    private int _selectedModelIdx = 0;
	    private int _selectedModelDataClassIdx = 0;
        private int _modelsAmount;
        //
        private int _selectedItemDataClsIdx = 0;
        private int _selectedItemIdx = 0;
        private int _itemsAmount;

        private string _bpId = string.Empty;
        private long _bpPoints = 0;

        private int _selectedUserIdx = 0;

        private Dictionary<string, JObject> _callData = new(); 

        private enum TabIndices
        {
	        USERS_TAB_IDX,
            REQ_HANDLERS_TAB_IDX,
            JOB_HANDLERS_TAB_IDX,
            MODELS_TAB_IDX,
            ITEMS_TAB_IDX,
            GM_TAB_IDX,
        };

        private int _toolbarIdx = 0;
        private string[] _toolbarStrings = {
	        "Users",
            "Requests",
	        "Jobs",
            "Models",
            "Items",
            "Mechanics"
        };

        [MenuItem("HyperEdge/APITester")]
        public static void ShowWindow()
        {
            GetWindow<APITesterWindow>();
        }

        public void Awake()
        {
            _appData = AppDataManager.Default.CurrentAppData;
            InitCallbacks();
            //
            if (_appData is not null)
            {
                GetAppContainers();
            }
            StartHealthCheckLoop();
        }

        private void StartHealthCheckLoop()
        {
            UniTask.Create(async () => {
                while (true) {
                    if (_cts.Token.IsCancellationRequested) {
                        break;
                    }
                    if (_selectedServerIdx >= _serverList.Count)
                    {
                        await UniTask.Delay(1000, cancellationToken: _cts.Token);
                        continue;
                    }
                    var serverInfo = _serverList[_selectedServerIdx];
                    var serverClient = _serverClients[serverInfo.ServerId];
                    await serverClient.CheckHealthAsync();
                    await UniTask.Delay(10000, cancellationToken: _cts.Token);
                }
            });
        }

        private void InitCallbacks()
        {
            if (_bag is null)
            {
                var d = DisposableBag.CreateBuilder();
                MessageHub.Instance.OnContainersInfo.Subscribe(r =>
                {
                    if (_appData is null)
                    {
                        return;
                    }
                    _serverList.Clear();
                    _serverClients.Clear();
                    foreach (var cInfo in r.Containers)
                    {
                        var appVerData = _appData.Versions.Find(v => v.Id == cInfo.VersionId);
                        var appEnvData = _appData.AppEnvironments.Find(v => v.Id == cInfo.EnvId);
                        var serverInfo = new ServerInfo
                        {
                            ServerId = cInfo.ServerId,
                            AppId = cInfo.AppId,
                            EnvId = cInfo.EnvId,
                            EnvName = appEnvData.Name,
                            VersionId = cInfo.VersionId,
                            VersionName = appVerData.Name,
                            Url = "https://" + HyperEdgeConstants.BackendUrl + ":5003"
                        };
                        _serverList.Add(serverInfo);
                        _serverClients.Add(serverInfo.ServerId, new ServerClient(serverInfo));
                    }
                    Debug.Log($"{_serverList.Count} servers online");
                }).AddTo(d);
                //
                MessageHub.Instance.OnServerHealthInfo.Subscribe(msg => {
                    _isServerHealthy = msg.Healthy;
                    Debug.Log($"Is Healthy: {msg.Healthy}");
                }).AddTo(d);
                _bag = d.Build();
            }
            if (_heClient is null)
            {
                _heClient = new HyperEdgeBackendClient();
            }
        }

        public void OnDestroy()
        {
            _cts?.Cancel();
            _bag?.Dispose();
        }

        private void GetAppContainers()
        {
            _heClient.GetAppContainers(_appData.Id).AsUniTask().Forget();
        }

        private void OnGUI()
        {
            //Awake();
            if (_appData is null)
            {
                EditorGUILayout.LabelField($"No application loaded");
                return;
            }
            //
            if (_selectedServerIdx >= _serverList.Count)
            {
                EditorGUILayout.LabelField($"No running servers");
                return;
            }
            //
            var serverNames = _serverList.Select(s => $"{s.VersionName}-{s.EnvName}").ToArray();
            _selectedServerIdx  = EditorGUILayout.Popup(_selectedServerIdx, serverNames);
            var serverInfo = _serverList[_selectedServerIdx];
            //
            if (_appDef is null)
            {
                _appDef = AppDefCache.Instance.GetAppDef(_appData.Name, serverInfo.VersionName);
            }
            EditorGUILayout.LabelField($"Running Server version={_serverList[_selectedServerIdx].VersionName}");
            EditorGUILayout.LabelField($"Server health: {_isServerHealthy}");
            //
            _toolbarIdx = GUILayout.Toolbar(_toolbarIdx, _toolbarStrings);
	        if (_toolbarIdx == (int)TabIndices.USERS_TAB_IDX)
            {
                OnUsersView();
            }
            else if (_toolbarIdx == (int)TabIndices.REQ_HANDLERS_TAB_IDX)
            {
                OnRequestHandlersView();
            }
            else if (_toolbarIdx == (int)TabIndices.JOB_HANDLERS_TAB_IDX)
            {
		        OnJobHandlersView();
	        }
            else if (_toolbarIdx == (int)TabIndices.MODELS_TAB_IDX)
            {
                OnModelsView();
            }
            else if (_toolbarIdx == (int)TabIndices.ITEMS_TAB_IDX)
            {
                OnItemsView();
            }
            else if (_toolbarIdx == (int)TabIndices.GM_TAB_IDX)
            {
                OnMechanicsView();
            }
	    }

        private LocalAccountRepo GetLocalAccountRepo(string envId)
        {
            if (!_accountRepos.TryGetValue(envId, out var repo))
            {
                repo = new LocalAccountRepo(envId);
                _accountRepos[envId] = repo;
            }
            return repo;
        }

        private void OnUsersView()
        {
            var serverInfo = _serverList[_selectedServerIdx];
            var serverClient = _serverClients[serverInfo.ServerId];
            var loggedIn = serverClient.Account is not null;
            var userRepo = GetLocalAccountRepo(serverInfo.EnvId);
            //
            var userIds = userRepo.Accounts.Select(x => x.UserId).ToArray();
            if (userIds.Length > 0)
            {
                _selectedUserIdx = EditorGUILayout.Popup(_selectedUserIdx, userIds);
            }
            //
            if (GUILayout.Button("Login"))
            {
                SwitchToAccount(userIds[_selectedUserIdx], serverInfo, userRepo).Forget();
            }
            if (loggedIn && GUILayout.Button("Show"))
            {
                ShowUser(userIds[_selectedUserIdx], serverInfo, userRepo).Forget();
            }
            //
            if (GUILayout.Button("New User"))
            {
                CreateNewAccount(serverInfo, userRepo).Forget();
            }
            //
            if (GUILayout.Button("Fetch users"))
            {
                _heClient.GetAppUsers(_appData.Id).Forget();
            }
        }

        private async UniTaskVoid SwitchToAccount(string userId, ServerInfo serverInfo, LocalAccountRepo userRepo)
        {
            var serverClient = _serverClients[serverInfo.ServerId];
            var authHelper = new AuthHelper(serverClient, userRepo);
            var accData = await authHelper.SwitchAccount(userId);
            Debug.Log($"Logged to account: {accData.Email}");
            serverClient.SetAccount(accData);
        }

        private async UniTaskVoid ShowUser(string userId, ServerInfo serverInfo, LocalAccountRepo userRepo)
        {
            var serverClient = _serverClients[serverInfo.ServerId];
            var userData = await serverClient.GetCurrentUserAsync();
            var jObj = JsonConvert.DeserializeObject<JObject>(userData);
            JsonPopupWindow.ShowWindow($"User {userId}", jObj["User"].ToObject<JObject>());
        }

        private async UniTaskVoid CreateNewAccount(ServerInfo serverInfo, LocalAccountRepo userRepo)
        {
            var serverClient = _serverClients[serverInfo.ServerId];
            var authHelper = new AuthHelper(serverClient, userRepo);
            var accData = await authHelper.CreateNewAccountAsync();
            Debug.Log($"Registered new account: {accData.Email}");
        }

        private void OnRequestHandlersView()
	    {
            if (_appDef is null)
            {
                return; 
            }
            var reqHandlers = _appDef.Data.RequestHandlers.Select(v => v.Name).ToArray();
	        _selectedReqHandlerIdx = EditorGUILayout.Popup(_selectedReqHandlerIdx, reqHandlers);
            if (_selectedReqHandlerIdx >= _appDef.Data.RequestHandlers.Count)
            {
                return;
            }
	        var reqHandlerData = _appDef.Data.RequestHandlers[_selectedReqHandlerIdx];
            //
            EditorGUILayout.Space();
            var reqClassData = _appDef.Data.StructClasses.Find(v => v.Name == reqHandlerData.RequestClassName);
	        if (reqClassData is null)
            {
                return;
            }
            if (!_callData.TryGetValue(reqHandlerData.Name, out var callData))
            {
                callData = new JObject();
                _callData[reqHandlerData.Name] = callData;
            }
            foreach (var fldData in reqClassData.Fields)
	        {
         	    EditorGUILayout.LabelField(fldData.Name + ":");
                var fldVal = EditorGUILayout.TextField(callData[fldData.Name]?.Value<string>() ?? "");
                callData[fldData.Name] = fldVal;
            }
	        EditorGUILayout.Space();
            //
            if (GUILayout.Button("Send"))
            {
                var serverInfo = _serverList[_selectedServerIdx];
                _serverClients[serverInfo.ServerId].CallServerHandler(reqHandlerData.Name, callData);
            }
        }

        private void OnJobHandlersView()
	    {
            if (_appDef is null)
            {
                return; 
            }
            var jobHandlers = _appDef.Data.JobHandlers.Select(v => v.Name).ToArray();
            _selectedJobHandlerIdx = EditorGUILayout.Popup(_selectedJobHandlerIdx, jobHandlers);
            if (_selectedJobHandlerIdx >= _appDef.Data.JobHandlers.Count)
            {
                return;
            }
            //
            EditorGUILayout.Space();
	        var jobHandlerData = _appDef.Data.JobHandlers[_selectedJobHandlerIdx];
            var jobClassData = _appDef.Data.StructClasses.Find(v => v.Name == jobHandlerData.JobDataClassName);
	        if (jobClassData is null)
            {
                return;
            }
            foreach (var fldData in jobClassData.Fields)
	        {
         	    EditorGUILayout.LabelField(fldData.Name + ":");
                EditorGUILayout.TextField("");
            }
	        EditorGUILayout.Space();
            //
	    }

        private void OnModelsView()
        {
            if (_appDef is null)
            {
                return; 
            }

            var modelClasses = _appDef.Data.ModelClasses.Select(v => v.Name).ToArray();
            _selectedModelIdx = EditorGUILayout.Popup(_selectedModelIdx, modelClasses);
            if (_selectedModelIdx >= _appDef.Data.ModelClasses.Count)
            {
                return;
            }
            var modelClassData = _appDef.Data.ModelClasses[_selectedModelIdx];
            if (modelClassData is null)
            {
                return;
            }
	        EditorGUILayout.Space();
            //
            var dataClsInsts = _appDef.GetDataClassInstancesByModelName(modelClassData.Name);
            if (dataClsInsts is null)
            {
                return;
            }
            //
            var instanceNames = dataClsInsts.Select(v => v.Name).ToArray();
            _selectedModelDataClassIdx = EditorGUILayout.Popup(_selectedModelDataClassIdx, instanceNames);
            //
            EditorGUILayout.LabelField("Amount:");
            _modelsAmount = EditorGUILayout.IntField(_modelsAmount);
            //
            if (GUILayout.Button("Add"))
            {
                var serverInfo = _serverList[_selectedServerIdx];
                _serverClients[serverInfo.ServerId].AddEntity(
                    modelClassData.Name,
                    dataClsInsts[_selectedModelDataClassIdx].Id.ToString(),
                    (ulong)_modelsAmount).Forget();
            }

	    }

        private void OnItemsView()
        {
            if (_appDef is null)
            {
                return; 
            }
            //
            var dataClasses = _appDef.Data.DataClasses.Select(v => v.Name).ToArray();
            _selectedItemDataClsIdx = EditorGUILayout.Popup(_selectedItemDataClsIdx, dataClasses);
            if (_selectedItemDataClsIdx >= _appDef.Data.DataClasses.Count)
            {
                return;
            }
            //
            var dataCls = _appDef.Data.DataClasses[_selectedItemDataClsIdx];
            var dataClsInsts = _appDef.GetDataClassInstancesByName(dataCls.Name);
            if (dataClsInsts is null)
            {
                return;
            }
            var instNames = dataClsInsts.Select(v => v.Name).ToArray();
            _selectedItemIdx = EditorGUILayout.Popup(_selectedItemIdx, instNames);
            if (_selectedItemIdx >= dataClsInsts.Count)
            {
                return;
            }
            var dci = dataClsInsts[_selectedItemIdx];
            //
            EditorGUILayout.LabelField("Amount:");
            _itemsAmount = EditorGUILayout.IntField(_itemsAmount);
            //
            if (GUILayout.Button("Add"))
            {
                Debug.Log($"Sending: {dci.Id.ToString()}");
                var serverInfo = _serverList[_selectedServerIdx];
                _serverClients[serverInfo.ServerId].AddItems(
                    dci.Id.ToString(),
                    (ulong)_itemsAmount).Forget();
            }
        }

        private void OnMechanicsView()
        {
            var serverInfo = _serverList[_selectedServerIdx];
            var serverClient = _serverClients[serverInfo.ServerId];
            //
            EditorGUILayout.LabelField("Add BattlePass points");
            //
            EditorGUILayout.LabelField("BattlePass Id:");
            _bpId = EditorGUILayout.TextField(_bpId);
            EditorGUILayout.LabelField("Points:");
            _bpPoints = EditorGUILayout.IntField((int)_bpPoints);
            //
            if (GUILayout.Button("Add BattlePass Points"))
            {
                serverClient.AddBattlePassScore(_bpId, _bpPoints).Forget();
            }
        }
    }
}

