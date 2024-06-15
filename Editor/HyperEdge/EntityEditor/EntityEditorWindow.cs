using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MessagePipe;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEditor;

using HyperEdge.Shared.Protocol.Models.Export;


namespace HyperEdge.Sdk.Unity.EntityEditor
{
    public class EntityEditorWindow : EditorWindow
    {
        private AppData? _appData = null;
        private AppDef? _appDef = null;
        private HyperEdgePy? _py = null; 
        // 
        private IDisposable _bag;        

        private List<DataClassEditHelper> _helpers = new();

        private int _actionIdx = 0;
        private string[] _actionNames = new string[] {
            "Create new",
            "Edit",
            "Remove",
        };
        private enum EditorAction {
            CREATE,
            EDIT,
            REMOVE
        };

        private SemaphoreSlim _taskSem = new SemaphoreSlim(1, 1);
        private bool TaskInProgress { get => _taskSem.CurrentCount == 0; }

        private int _newGmTypeIdx = 0;
        private int _newGmStructTypeIdx = 0;

        private string _newEsName = "";
        private string _newBpName = "";
        private string _newTournamentName = "";

        private bool _isExpLadder = false;
        private string _newLadderName = "";
        private int _ladderModelClassIdx = 0;

        private string _newUserGroupName = "";

        private string _newRequestHandlerName = "";
        private string _newJobHandlerName = "";

        private List<DataClassDTO> _newDataClass = new();

        private int _dataClassIdx = -1;
        private int _modelClassIdx = 0;

        private int _bpIdx = 0;
        private int _esIdx = 0;
        private int _lbIdx = 0;

        private int _rhIdx = 0;        
        private int _rhReqStIdx = 0;
        private int _rhRespStIdx = 0;

        enum GmStructType
        {
            STATIC_DATA = 0,
            MODEL,
            STRUCT,
            STORAGE,
            EVENT,
        };

        private string[] _gm_struct_types = new string[]
        {
            "Game Data",
            "Game Models",
            "Structs",
            "Storage",
            "Events"
        };

        enum GmType
        {
            GAME_DATA = 0,
            LADDER,
            GROUP,
            ENERGY_SYSTEM,
            BATTLE_PASS,
            REQUEST_HANDLER,
            JOB_HANDLER,
            TOURNAMENT, 
        };

        private string[] _gm_typenames = new string[] {
            "Data Models",
            "Ladders",
            "User groups",
            "EnergySystems",
            "BattlePasses",
            "RequestHandler",
            "JobHandler",
            "Tournaments",
        };

        private int _selectedStorageTypeIdx = -1;
        private string[] _gm_storage_types = new string[] {
            "UserVariable",
            "UserStorage",
            "GlobalVariable",
            "GlobalStorage"
        };

        [MenuItem("HyperEdge/EntityEditor")]
        public static void ShowWindow()
        {
            var wnd = GetWindow(typeof(EntityEditorWindow));
            wnd.Show();
        }

        public void Awake()
        {
            if (_appData is null)
            {
                _appData = AppDataManager.Default.CurrentAppData;
            }
            if (_appData is null)
            {
                _appData = AppData.Load();
            }
            if (_appData is null)
            {
                return;
            }
            //
            _py = new HyperEdgePy(_appData.Name);
            //
            LoadAppDef();

            if (_bag is null)
            {
                var d = DisposableBag.CreateBuilder();
                AppDefCache.Instance.OnAppDefLoaded.Subscribe(msg => OnAppDefLoaded(msg)).AddTo(d);
                _bag = d.Build();
            }
        }

        public void Destroy()
        {
            _bag.Dispose();
        }

        private void LoadAppDef()
        {
            _helpers.Clear();
            _newDataClass.Clear();
            _appDef = AppDefCache.Instance.GetCurrentAppDef(_appData.Name);
            if (_appDef is null)
            {
                return;
            }
            //
            for (int i = 0; i < 4; i++)
            {
                _helpers.Add(new DataClassEditHelper(_appDef, null));
                _newDataClass.Add(new DataClassDTO());
            }
            _dataClassIdx = -1;
        }

        private void OnAppDefLoaded(OnAppDefLoadedMsg msg)
        {
            if (_appData is not null &&
                msg.AppName == _appData.Name &&
                msg.VersionName == "current")
            {
                LoadAppDef();
            }
        }

        private void OnGUI()
        {
            if (_appDef is null)
            {
                EditorGUILayout.LabelField("Can't load any application definition", EditorStyles.boldLabel);
                return;
            }
            // Display current data info
            EditorGUILayout.LabelField("Current App:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("AppId:");
            EditorGUILayout.LabelField(_appData.Id);
            EditorGUILayout.EndHorizontal();
            //
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name:");
            EditorGUILayout.LabelField(_appData.Name);
            EditorGUILayout.EndHorizontal();
            //
            _actionIdx = EditorGUILayout.Popup(_actionIdx, _actionNames);
            //
            _newGmTypeIdx = EditorGUILayout.Popup(_newGmTypeIdx, _gm_typenames);
            if (_newGmTypeIdx == (int)GmType.GAME_DATA)
            {
                OnGameModelsTab();
            }
            else if (_newGmTypeIdx == (int)GmType.LADDER)
            {
                OnLaddersTab();
            }
            else if (_newGmTypeIdx == (int)GmType.GROUP)
            {
                OnUserGroupTab();
            }
            else if (_newGmTypeIdx == (int)GmType.BATTLE_PASS)
            {
                OnBattlePassesTab();
            }
            else if (_newGmTypeIdx == (int)GmType.ENERGY_SYSTEM)
            {
                OnEnergySystemsTab();
            }
            else if (_newGmTypeIdx == (int)GmType.REQUEST_HANDLER)
            {
                OnRequestHandlersTab();
            }
            else if (_newGmTypeIdx == (int)GmType.JOB_HANDLER)
            {
                OnJobHandlersTab();
            }
            else if (_newGmTypeIdx == (int)GmType.TOURNAMENT)
            {
                OnTournamentsTab();
            }
        }

        private void OnGameModelsTab()
        {
            _newGmStructTypeIdx = EditorGUILayout.Popup(_newGmStructTypeIdx, _gm_struct_types);
            if (_actionIdx == (int)EditorAction.CREATE)
            {
                _helpers[0].SetDataClass(_newDataClass[0]);
                _helpers[0].RenderDataClassEditGUI(); 
                //
                EditorGUILayout.Space();
                //
                GUI.enabled = !TaskInProgress;
                if (_newGmStructTypeIdx == (int)GmStructType.STATIC_DATA)
                {
                    if (GUILayout.Button("Create GameData"))
                    {
                        CreateNewDataClass(_newDataClass[0], false).Forget();
                    }
                }
                else if (_newGmStructTypeIdx == (int)GmStructType.MODEL)
                {
                    EditorGUILayout.LabelField("Static data fields:");
                    //
                    _helpers[1].SetDataClass(_newDataClass[1]);
                    _helpers[1].RenderDataClassEditGUI(); 
                    //
                    EditorGUILayout.Space();
                    if (GUILayout.Button("Create DataModel"))
                    {
                        CreateNewModelClass(_newDataClass[1], _newDataClass[0]).Forget();
                    }
                }
                else if (_newGmStructTypeIdx == (int)GmStructType.STRUCT)
                {
                    if (GUILayout.Button("Create struct"))
                    {
                        CreateNewStruct(_newDataClass[0], false).Forget();
                    }
                }
                else if (_newGmStructTypeIdx == (int)GmStructType.STORAGE)
                {
                    EditorGUILayout.Space();
                    //
                    _selectedStorageTypeIdx = EditorGUILayout.Popup(_selectedStorageTypeIdx, _gm_storage_types);
                    if (GUILayout.Button("Create Storage"))
                    {
                        CreateNewStorageClass(_newDataClass[0], _gm_storage_types[_selectedStorageTypeIdx], false).Forget();
                    }
                }
                else if (_newGmStructTypeIdx == (int)GmStructType.EVENT)
                {
                    if (GUILayout.Button("Create Event"))
                    {
                        CreateNewEventClass(_newDataClass[0], false).Forget();
                    }
                }
                GUI.enabled = true;
            }
            else if (_actionIdx == (int)EditorAction.EDIT)
            {
                if (_newGmStructTypeIdx == (int)GmStructType.STATIC_DATA)
                {
                    var dataClassNames = _appDef.Data.DataClasses.Select(v => v.Name).ToArray();
                    var dataClassIdx = EditorGUILayout.Popup(_dataClassIdx, dataClassNames);
                    if (dataClassIdx != _dataClassIdx)
                    {
                        var dataClass = _appDef.Data.DataClasses[dataClassIdx];
                        _newDataClass[0] = dataClass.Clone();
                        _helpers[0].SetDataClass(_newDataClass[0]);
                        _dataClassIdx = dataClassIdx;
                    }
                    _helpers[0].RenderDataClassEditGUI(); 
                    //
                    EditorGUILayout.Space();
                    if (GUILayout.Button("Update"))
                    {
                        UpdateNewDataClass(_newDataClass[0]).Forget();
                    }
                }
                else if (_newGmStructTypeIdx == (int)GmStructType.MODEL)
                {
                    var modelClassNames = _appDef.Data.ModelClasses.Select(v => v.Name).ToArray();
                    _modelClassIdx = EditorGUILayout.Popup(_modelClassIdx, modelClassNames);
                    var modelClass = _appDef.Data.ModelClasses[_modelClassIdx];
                    _helpers[0].SetDataClass(modelClass);
                    _helpers[0].RenderDataClassEditGUI(); 
                    //
                    EditorGUILayout.Space();
                    if (GUILayout.Button("Update"))
                    {
                    }
                }
                else if (_newGmStructTypeIdx == (int)GmStructType.STRUCT)
                {
                    var modelClassNames = _appDef.Data.StructClasses.Select(v => v.Name).ToArray();
                    _modelClassIdx = EditorGUILayout.Popup(_modelClassIdx, modelClassNames);
                    var modelClass = _appDef.Data.StructClasses[_modelClassIdx];
                    _helpers[0].SetDataClass(modelClass);
                    _helpers[0].RenderDataClassEditGUI(); 
                    //
                    EditorGUILayout.Space();
                    if (GUILayout.Button("Update Struct"))
                    {
                        CreateNewStruct(_newDataClass[0], true).Forget();
                    }
                }
                else if (_newGmStructTypeIdx == (int)GmStructType.STORAGE)
                {
                    //
                    EditorGUILayout.Space();
                    if (GUILayout.Button("Update Event Struct"))
                    {
                        CreateNewStorageClass(_newDataClass[0], _gm_storage_types[_selectedStorageTypeIdx], true).Forget();
                    }
                }
                else if (_newGmStructTypeIdx == (int)GmStructType.EVENT)
                {
                    var modelClassNames = _appDef.Data.EventClasses.Select(v => v.Name).ToArray();
                    _modelClassIdx = EditorGUILayout.Popup(_modelClassIdx, modelClassNames);
                    var modelClass = _appDef.Data.StructClasses[_modelClassIdx];
                    _helpers[0].SetDataClass(modelClass);
                    _helpers[0].RenderDataClassEditGUI(); 
                    //
                    EditorGUILayout.Space();
                    if (GUILayout.Button("Update Event Struct"))
                    {
                        CreateNewEventClass(_newDataClass[0], true).Forget();
                    }
                }
            }
        }

        private void OnBattlePassesTab()
        {
            if (_actionIdx == (int)EditorAction.CREATE)
            {
                EditorGUILayout.LabelField("New BattlePass name:");
                _newBpName = EditorGUILayout.TextField(_newBpName);
                EditorGUILayout.Space();
                //
                EditorGUILayout.LabelField("Static data fields:");
                _helpers[0].SetDataClass(_newDataClass[0]);
                _helpers[0].RenderDataClassEditGUI();
                EditorGUILayout.Space();
                //
                EditorGUILayout.LabelField("Per level data fields:");
                _helpers[1].SetDataClass(_newDataClass[1]);
                _helpers[1].RenderDataClassEditGUI();
                EditorGUILayout.Space();
                //
                EditorGUILayout.LabelField("Dynamic fields:");
                _helpers[2].SetDataClass(_newDataClass[2]);
                _helpers[2].RenderDataClassEditGUI();
                EditorGUILayout.Space();
                //
                GUI.enabled = !TaskInProgress;
                if (GUILayout.Button("Create BattlePass"))
                {
                    CreateNewBattlePass(_newBpName, _newDataClass[0], _newDataClass[1], _newDataClass[2], false).Forget();
                }
                GUI.enabled = true;
            }
            else if (_actionIdx == (int)EditorAction.EDIT)
            {
                var bpNames = _appDef.Data.BattlePasses.Select(v => v.Name).ToArray();
                var bpIdx = EditorGUILayout.Popup(_bpIdx, bpNames);
                if (bpIdx != _bpIdx)
                {
                    var bpData = _appDef.Data.BattlePasses[_bpIdx];
                    //
                    _newDataClass[0] = new DataClassDTO();
                    _newDataClass[0].Fields = bpData.Data.Clone().Fields;
                    _helpers[0].SetDataClass(_newDataClass[0]);
                    //
                    _newDataClass[1] = new DataClassDTO();
                    _newDataClass[1].Fields = bpData.LadderLevelData.Clone().Fields;
                    _helpers[1].SetDataClass(_newDataClass[1]);
                    //
                    _newDataClass[2] = new DataClassDTO();
                    _newDataClass[2].Fields = bpData.Model.Clone().Fields;
                    _helpers[2].SetDataClass(_newDataClass[2]);
                    //
                    _bpIdx = bpIdx;
                }
                //
                EditorGUILayout.LabelField("Static data:");
                _helpers[0].RenderDataClassEditGUI();
                EditorGUILayout.Space();
                _helpers[1].RenderDataClassEditGUI();
                EditorGUILayout.LabelField("Model fields:");
                _helpers[2].RenderDataClassEditGUI();
                EditorGUILayout.Space();
                //
                GUI.enabled = !TaskInProgress;
                if (GUILayout.Button("Update BattlePass"))
                {
                    CreateNewBattlePass(_newBpName, _newDataClass[0], _newDataClass[1], _newDataClass[2], true).Forget();
                }
                GUI.enabled = true;
            }
        }

        private void OnEnergySystemsTab()
        {
            if (_actionIdx == (int)EditorAction.CREATE)
            {
                EditorGUILayout.LabelField("New EnergySystem name:");
                _newEsName = EditorGUILayout.TextField(_newEsName);
                EditorGUILayout.Space();
                //
                EditorGUILayout.LabelField("Static data fields:");
                _helpers[0].SetDataClass(_newDataClass[0]);
                _helpers[0].RenderDataClassEditGUI();
                EditorGUILayout.Space();
                //
                EditorGUILayout.LabelField("Dynamic fields:");
                _helpers[1].SetDataClass(_newDataClass[1]);
                _helpers[1].RenderDataClassEditGUI();
                EditorGUILayout.Space();
                //
                GUI.enabled = !TaskInProgress;
                if (GUILayout.Button("Create EnergySystem"))
                {
                    CreateNewEnergySystem(_newEsName, _newDataClass[0], _newDataClass[1], false).Forget();
                }
                GUI.enabled = true;
            }
            else if (_actionIdx == (int)EditorAction.EDIT)
            {
                var esNames = _appDef.Data.EnergySystems.Select(v => v.Name).ToArray();
                var esIdx = EditorGUILayout.Popup(_esIdx, esNames);
                if (esIdx != _esIdx)
                {
                    var esData = _appDef.Data.EnergySystems[_esIdx];
                    //
                    _newDataClass[0] = new DataClassDTO();
                    _newDataClass[0].Fields = esData.Model.Fields;
                    _helpers[0].SetDataClass(_newDataClass[0]);
                    //
                    _newDataClass[1] = new DataClassDTO();
                    _newDataClass[1].Fields = esData.Data.Fields;
                    _helpers[1].SetDataClass(_newDataClass[1]);
                    //
                    _esIdx = esIdx;
                }
                EditorGUILayout.LabelField("Dynamic fields:");
                _helpers[0].RenderDataClassEditGUI();
                EditorGUILayout.Space();
                //
                EditorGUILayout.LabelField("Static data fields:");
                _helpers[1].RenderDataClassEditGUI();
                EditorGUILayout.Space();
                //
                GUI.enabled = !TaskInProgress;
                if (GUILayout.Button("Update EnergySystem"))
                {
                    CreateNewEnergySystem(_newEsName, _newDataClass[0], _newDataClass[1], true).Forget();
                }
                GUI.enabled = true;
            }
        }

        private void OnLaddersTab()
        {
            if (_actionIdx == (int)EditorAction.CREATE)
            {
                if (_appDef.Data.ModelClasses.Count == 0)
                {
                    EditorGUILayout.LabelField("No GameModels defined", EditorStyles.boldLabel);
                    return;
                }
                EditorGUILayout.LabelField("New Ladder name:");
                _newLadderName = EditorGUILayout.TextField(_newLadderName);
                EditorGUILayout.Space();
                //
                var modelClassNames = _appDef.Data.ModelClasses.Select(v => v.Name).ToArray();
                _ladderModelClassIdx = EditorGUILayout.Popup(_ladderModelClassIdx, modelClassNames);
                if (_ladderModelClassIdx >= _appDef.Data.ModelClasses.Count)
                {
                    return;
                }
                var modelClass = _appDef.Data.ModelClasses[_ladderModelClassIdx];
                EditorGUILayout.Space();
                //
                _isExpLadder = EditorGUILayout.Toggle("Is experience based:", _isExpLadder);
                //
                EditorGUILayout.LabelField("Per level data fields:");
                _helpers[0].SetDataClass(_newDataClass[0]);
                _helpers[0].RenderDataClassEditGUI();
                EditorGUILayout.Space();
                //
                GUI.enabled = !TaskInProgress;
                if (GUILayout.Button("Create Ladder"))
                {
                    CreateNewLadder(modelClass.Name, _newDataClass[0], _isExpLadder).Forget();
                }
                GUI.enabled = true;
            }
        }

        private void OnUserGroupTab()
        {
            if (_actionIdx == (int)EditorAction.CREATE)
            {
                EditorGUILayout.LabelField("New User Group name:");
                _newUserGroupName = EditorGUILayout.TextField(_newUserGroupName);
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Model fields:");
                _helpers[0].SetDataClass(_newDataClass[0]);
                _helpers[0].RenderDataClassEditGUI();
                EditorGUILayout.Space();
                //
                GUI.enabled = !TaskInProgress;
                if (GUILayout.Button("Create User Group Model"))
                {
                    CreateNewUserGroup(_newUserGroupName, _newDataClass[0]).Forget();
                }
                GUI.enabled = true;
            }
        }

        private void OnTournamentsTab()
        {
            if (_actionIdx == (int)EditorAction.CREATE)
            {
                EditorGUILayout.LabelField("New Tournament name:");
                _newTournamentName = EditorGUILayout.TextField(_newTournamentName);
                EditorGUILayout.Space();
                //
                EditorGUILayout.LabelField("Static data fields:");
                _helpers[0].SetDataClass(_newDataClass[0]);
                _helpers[0].RenderDataClassEditGUI();
                EditorGUILayout.Space();
                //
                EditorGUILayout.LabelField("Dynamic fields:");
                _helpers[1].SetDataClass(_newDataClass[1]);
                _helpers[1].RenderDataClassEditGUI();
                EditorGUILayout.Space();
                //
                GUI.enabled = !TaskInProgress;
                if (GUILayout.Button("Create Tournament"))
                {
                    CreateNewTournament(_newTournamentName, _newDataClass[0], _newDataClass[1], false).Forget();
                }
                GUI.enabled = true;
            }
            else if (_actionIdx == (int)EditorAction.EDIT)
            {

                var lbNames = _appDef.Data.Tournaments.Select(v => v.Name).ToArray();
                var lbIdx = EditorGUILayout.Popup(_lbIdx, lbNames);
                if (lbIdx != _lbIdx)
                {
                    var lbData = _appDef.Data.Tournaments[_lbIdx];
                    //
                    _newDataClass[0] = new DataClassDTO();
                    _newDataClass[0].Fields = lbData.Data.Fields;
                    _helpers[0].SetDataClass(_newDataClass[0]);
                    //
                    _newDataClass[1] = new DataClassDTO();
                    _newDataClass[1].Fields = lbData.Model.Fields;
                    _helpers[1].SetDataClass(_newDataClass[1]);
                    //
                    _lbIdx = lbIdx;
                }
                //
                EditorGUILayout.LabelField("Static data fields:");
                _helpers[0].RenderDataClassEditGUI();
                EditorGUILayout.Space();
                //
                EditorGUILayout.LabelField("Dynamic fields:");
                _helpers[1].RenderDataClassEditGUI();
                EditorGUILayout.Space();
                //
                GUI.enabled = !TaskInProgress;
                if (GUILayout.Button("Update Tournament"))
                {
                    CreateNewTournament(_newTournamentName, _newDataClass[0], _newDataClass[1], true).Forget();
                }
                GUI.enabled = true;
            }
        }

        private void OnRequestHandlersTab()
        {
            if (_actionIdx == (int)EditorAction.CREATE)
            {
                EditorGUILayout.LabelField("New RequestHandler name:");
                _newRequestHandlerName = EditorGUILayout.TextField(_newRequestHandlerName);
                EditorGUILayout.Space();
                //
                EditorGUILayout.LabelField("Request struct fields:");
                _helpers[0].SetDataClass(_newDataClass[0]);
                _helpers[0].RenderDataClassEditGUI();
                EditorGUILayout.Space();
                //
                EditorGUILayout.LabelField("Response struct fields:");
                _helpers[1].SetDataClass(_newDataClass[1]);
                _helpers[1].RenderDataClassEditGUI();
                EditorGUILayout.Space();
                //
                GUI.enabled = !TaskInProgress;
                if (GUILayout.Button("Create Request handler"))
                {
                    CreateNewRequestHandler(_newRequestHandlerName, _newDataClass[0], _newDataClass[1]).Forget();
                }
                GUI.enabled = true;
            }
            else 
            {
                var stNames = _appDef.Data.StructClasses.Select(v => v.Name).ToArray();
                var rhNames = _appDef.Data.RequestHandlers.Select(v => v.Name).ToArray();
                //
                _rhIdx = EditorGUILayout.Popup(_rhIdx, rhNames);
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Request data:");
                _rhReqStIdx = EditorGUILayout.Popup(_rhReqStIdx, stNames);
                var reqStData = _appDef.Data.StructClasses[_rhReqStIdx];
                _helpers[0].SetDataClass(reqStData);
                _helpers[0].RenderDataClassEditGUI(); 
                EditorGUILayout.Space();
                //
                EditorGUILayout.LabelField("Response data:");
                _rhRespStIdx = EditorGUILayout.Popup(_rhRespStIdx, stNames);
                var respStData = _appDef.Data.StructClasses[_rhRespStIdx];
                _helpers[1].SetDataClass(respStData);
                _helpers[1].RenderDataClassEditGUI(); 
                EditorGUILayout.Space();
                //
                EditorGUILayout.Space();
                if (GUILayout.Button("Update"))
                {
                }
            }
        }

        private void OnJobHandlersTab()
        {
            if (_actionIdx == (int)EditorAction.CREATE)
            {
                EditorGUILayout.LabelField("New JobHandler name:");
                _newJobHandlerName = EditorGUILayout.TextField(_newJobHandlerName);
                EditorGUILayout.Space();
                //
                EditorGUILayout.LabelField("Job data fields:");
                _helpers[0].SetDataClass(_newDataClass[0]);
                _helpers[0].RenderDataClassEditGUI();
                EditorGUILayout.Space();
                //
                GUI.enabled = !TaskInProgress;
                if (GUILayout.Button("Create Job handler"))
                {
                    CreateNewJobHandler(_newJobHandlerName, _newDataClass[0]).Forget();
                }
                GUI.enabled = true;
            }  
        }

        private UniTask<bool> UpdateNewDataClass(DataClassDTO dataClass)
        {
            return CreateNewDataClass(dataClass, true);
        }

        private async UniTask<bool> CreateNewDataClass(DataClassDTO dataClass, bool update)
        {
            await _taskSem.WaitAsync();
            try
            {
                return await CreateNewDataClassImpl(dataClass, update);
            }
            finally
            {
                _taskSem.Release();
            }
        }

        private async UniTask<bool> CreateNewDataClassImpl(DataClassDTO dataClass, bool update)
        {
            var fldsArray = dataClass.Fields.Select(f => $"{f.Name}:{f.Typename}").ToArray();
            var pyRet = await _py.CreateDataClass(dataClass.Name, String.Join(',', fldsArray), update);
            string actionName = update ? "update" : "create";
            if (!pyRet.IsSuccess)
            {
                EditorUtility.DisplayDialog("HyperEdge", $"Failed to {actionName} DataClass \"{dataClass.Name}\"", "Ok");
            }
            EditorUtility.DisplayDialog("HyperEdge", $"Successfully {actionName}d DataClass \"{dataClass.Name}\"", "Ok");
            return pyRet.IsSuccess;
        }

        private async UniTask<bool> CreateNewModelClass(DataClassDTO dataClass, DataClassDTO modelClass)
        {
            await _taskSem.WaitAsync();
            try
            {
                return await CreateNewModelClassImpl(dataClass, modelClass);
            }
            finally
            {
                _taskSem.Release();
            }
        }

        private async UniTask<bool> CreateNewModelClassImpl(DataClassDTO dataClass, DataClassDTO modelClass)
        {
            var dataFldsArray = dataClass.Fields.Select(f => $"{f.Name}:{f.Typename}").ToArray();
            var modelFldsArray = modelClass.Fields.Select(f => $"{f.Name}:{f.Typename}").ToArray();
            var pyRet = await _py.CreateModelClass(
                modelClass.Name,
                String.Join(',', dataFldsArray),
                String.Join(',', modelFldsArray));
            if (!pyRet.IsSuccess)
            {
                EditorUtility.DisplayDialog("HyperEdge", $"Failed to create ModelClass \"{name}\"", "Ok");
            }
            return pyRet.IsSuccess;
        }

        private async UniTask<bool> CreateNewStruct(DataClassDTO dataClass, bool update)
        {
            await _taskSem.WaitAsync();
            try
            {
                return await CreateNewStructImpl(dataClass, update);
            }
            finally
            {
                _taskSem.Release();
            }
        }

        private async UniTask<bool> CreateNewStructImpl(DataClassDTO dataClass, bool update)
        {
            var fldsArray = dataClass.Fields.Select(f => $"{f.Name}:{f.Typename}").ToArray();
            var pyRet = await _py.CreateStruct(dataClass.Name, String.Join(',', fldsArray), update);
            if (!pyRet.IsSuccess)
            {
                EditorUtility.DisplayDialog("HyperEdge", $"Failed to create struct \"{name}\"", "Ok");
            }
            return pyRet.IsSuccess;
        }

        private async UniTask<bool> CreateNewStorageClass(DataClassDTO dataClass, string storageType, bool update)
        {
            await _taskSem.WaitAsync();
            try
            {
                return await CreateNewStorageClassImpl(dataClass, storageType, update);
            }
            finally
            {
                _taskSem.Release();
            }
        }

        private async UniTask<bool> CreateNewStorageClassImpl(DataClassDTO dataClass, string storageType, bool update)
        {
            var fldsArray = dataClass.Fields.Select(f => $"{f.Name}:{f.Typename}").ToArray();
            var pyRet = await _py.CreateStorageClass(
                dataClass.Name,
                storageType,
                String.Join(',', fldsArray),
                update);
            if (!pyRet.IsSuccess)
            {
                EditorUtility.DisplayDialog("HyperEdge", $"Failed to create Storage \"{name}\" of type {storageType}", "Ok");
            }
            return pyRet.IsSuccess;
        }

        private async UniTask<bool> CreateNewEventClass(DataClassDTO dataClass, bool update)
        {
            await _taskSem.WaitAsync();
            try
            {
                return await CreateNewEventClassImpl(dataClass, update);
            }
            finally
            {
                _taskSem.Release();
            }
        }

        private async UniTask<bool> CreateNewEventClassImpl(DataClassDTO dataClass, bool update)
        {
            var fldsArray = dataClass.Fields.Select(f => $"{f.Name}:{f.Typename}").ToArray();
            var pyRet = await _py.CreateEventClass(dataClass.Name, String.Join(',', fldsArray), update);
            if (!pyRet.IsSuccess)
            {
                EditorUtility.DisplayDialog("HyperEdge", $"Failed to create event \"{name}\"", "Ok");
            }
            return pyRet.IsSuccess;
        }

        private async UniTask<bool> CreateNewUserGroup(string name, DataClassDTO dataClass)
        {
            await _taskSem.WaitAsync();
            try
            {
                return await CreateNewUserGroupImpl(name, dataClass);
            }
            finally
            {
                _taskSem.Release();
            }
        }

        private async UniTask<bool> CreateNewUserGroupImpl(string name, DataClassDTO dataClass)
        {
            var fldsArray = dataClass.Fields.Select(f => $"{f.Name}:{f.Typename}").ToArray();
            var pyRet = await _py.CreateUserGroup(name, String.Join(',', fldsArray));
            if (!pyRet.IsSuccess)
            {
                EditorUtility.DisplayDialog("HyperEdge", $"Failed to create UserGroup \"{name}\"", "Ok");
            }
            return pyRet.IsSuccess;
        }

        private async UniTask<bool> CreateNewEnergySystem(string name, DataClassDTO dataClass, DataClassDTO modelClass, bool update)
        {
            await _taskSem.WaitAsync();
            try
            {
                return await CreateNewEnergySystemImpl(name, dataClass, modelClass, update);
            }
            finally
            {
                _taskSem.Release();
            }
        }

        private async UniTask<bool> CreateNewEnergySystemImpl(string name, DataClassDTO dataClass, DataClassDTO modelClass, bool update)
        {
            var dataFldsArray = dataClass.Fields.Select(f => $"{f.Name}:{f.Typename}").ToArray();
            var modelFldsArray = modelClass.Fields.Select(f => $"{f.Name}:{f.Typename}").ToArray();
            var pyRet = await _py.CreateEnergySystem(name, String.Join(',', dataFldsArray), String.Join(',', modelFldsArray), update);
            if (!pyRet.IsSuccess)
            {
                EditorUtility.DisplayDialog("HyperEdge", $"Failed to create EnergySystem \"{name}\"", "Ok");
            }
            return pyRet.IsSuccess;
        }

        private async UniTask<bool> CreateNewBattlePass(
                string name,
                DataClassDTO dataClass,
                DataClassDTO levelDataClass,
                DataClassDTO modelClass,
                bool update)
        {
            await _taskSem.WaitAsync();
            try
            {
                return await CreateNewBattlePassImpl(name, dataClass, levelDataClass, modelClass, update);
            }
            finally
            {
                _taskSem.Release();
            }
        }

        private async UniTask<bool> CreateNewBattlePassImpl(
                string name,
                DataClassDTO dataClass,
                DataClassDTO levelDataClass,
                DataClassDTO modelClass,
                bool update)
        {
            var dataFldsArray = dataClass.Fields.Select(f => $"{f.Name}:{f.Typename}").ToArray();
            var levelDataFldsArray = levelDataClass.Fields.Select(f => $"{f.Name}:{f.Typename}").ToArray();
            var modelFldsArray = modelClass.Fields.Select(f => $"{f.Name}:{f.Typename}").ToArray();
            var pyRet = await _py.CreateBattlePass(name,
                String.Join(',', dataFldsArray),
                String.Join(',', levelDataFldsArray),
                String.Join(',', modelFldsArray),
                update);
            if (!pyRet.IsSuccess)
            {
                EditorUtility.DisplayDialog("HyperEdge", $"Failed to create BattlePass \"{name}\"", "Ok");
            }
            return pyRet.IsSuccess;
        }

        private async UniTask<bool> CreateNewTournament(string name, DataClassDTO dataClass, DataClassDTO modelClass, bool update)
        {
            await _taskSem.WaitAsync();
            try
            {
                return await CreateNewTournamentImpl(name, dataClass, modelClass, update);
            }
            finally
            {
                _taskSem.Release();
            }
        }

        private async UniTask<bool> CreateNewTournamentImpl(
            string name,
            DataClassDTO dataClass,
            DataClassDTO modelClass,
            bool update)
        {
            var dataFldsArray = dataClass.Fields.Select(f => $"{f.Name}:{f.Typename}").ToArray();
            var modelFldsArray = modelClass.Fields.Select(f => $"{f.Name}:{f.Typename}").ToArray();
            var pyRet = await _py.CreateTournament(name, String.Join(',', dataFldsArray), String.Join(',', modelFldsArray), update);
            if (!pyRet.IsSuccess)
            {
                EditorUtility.DisplayDialog("HyperEdge", $"Failed to create tournament \"{name}\"", "Ok");
            }
            return pyRet.IsSuccess;
        }

        private async UniTask<bool> CreateNewLadder(string name, DataClassDTO levelDataClass, bool isExpBased)
        {
            await _taskSem.WaitAsync();
            try
            {
                return await CreateNewLadderImpl(name, levelDataClass, isExpBased);
            }
            finally
            {
                _taskSem.Release();
            }
        }

        private async UniTask<bool> CreateNewLadderImpl(string name, DataClassDTO levelDataClass, bool isExpBased)
        {
            var levelDataFldsArray = levelDataClass.Fields.Select(f => $"{f.Name}:{f.Typename}").ToArray();
            var pyRet = await _py.CreateLadder(name, isExpBased, String.Join(',', levelDataFldsArray));
            if (!pyRet.IsSuccess)
            {
                EditorUtility.DisplayDialog("HyperEdge", $"Failed to create ladder \"{name}\"", "Ok");
            }
            return pyRet.IsSuccess;
        }

        private async UniTask<bool> CreateNewRequestHandler(string name, DataClassDTO reqStruct, DataClassDTO respStruct)
        {
            await _taskSem.WaitAsync();
            try
            {
                return await CreateNewRequestHandlerImpl(name, reqStruct, respStruct);
            }
            finally
            {
                _taskSem.Release();
            }
        }

        private async UniTask<bool> CreateNewRequestHandlerImpl(string name, DataClassDTO reqStruct, DataClassDTO respStruct)
        {
            var reqFlds = reqStruct.Fields.Select(f => $"{f.Name}:{f.Typename}").ToArray();
            var respFlds = respStruct.Fields.Select(f => $"{f.Name}:{f.Typename}").ToArray();
            var pyRet = await _py.CreateRequestHandler(name, String.Join(',', reqFlds), String.Join(',', respFlds));
            return pyRet.IsSuccess;
        }

        private async UniTask<bool> CreateNewJobHandler(string name, DataClassDTO reqStruct)
        {
            await _taskSem.WaitAsync();
            try
            {
                return await CreateNewJobHandlerImpl(name, reqStruct);
            }
            finally
            {
                _taskSem.Release();
            }
        }

        private async UniTask<bool> CreateNewJobHandlerImpl(string name, DataClassDTO reqStruct)
        {
            var reqFlds = reqStruct.Fields.Select(f => $"{f.Name}:{f.Typename}").ToArray();
            var pyRet = await _py.CreateJobHandler(name, String.Join(',', reqFlds));
            return pyRet.IsSuccess;
        }
    }
}
