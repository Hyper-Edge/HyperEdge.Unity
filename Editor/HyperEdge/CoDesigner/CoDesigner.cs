using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json.Linq;
using MessagePipe;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEditor;

using HyperEdge.Shared.Protocol.Models.Export;


namespace HyperEdge.Sdk.Unity.CoDesigner
{
    public class CoDesignerWindow : EditorWindow
    {
        private static Dictionary<string, string> _GdSteps = new Dictionary<string, string>
        {
            {"propose_game_mechanics", "Generating Game Mechanics Proposals"},
            {"initial_propose_game_actions", "Generating User Action Proposals"},
            {"propose_entities", "Generating Game Entities Proposals"},
            {"propose_entity_types","Generating Game Entities subtypes Proposals"},
            {"propose_model", "Generating Structures for Game Entities Proposals"},
            {"categorize_upgradeables", "Categorizing Upgradeables"},
            {"initial_propose_mini_games", "Generating initial Mini-Games Proposals"},
            {"propose_craft", "Generating Crafting Proposals"},
            //
            {"propose_currencies", "Generating Virtual Currencies Proposals"},
            {"propose_resources", "Generating In-Game Resources Proposals"},
            {"propose_purchases", "Generating Purchases Proposals"},
            {"initial_propose_energy_systems", "Generating initial Energy Systems Proposals"},
            {"initial_propose_quest_types", "Generating initial Quest types Proposals"},
            {"initial_propose_achievement_types", "Generating initial Achievements Proposals" },
            {"initial_propose_social_mechanics", "Generating initial Social Mechanics Proposals"},
        };

        private AppData? _appData = null;
        private AppDef? _appDef = null;
        private HyperEdgePy? _py = null; 
        private HyperEdgeBackendClient _client = null;
        // 
        private IDisposable _bag;
        //
        private bool _gdDecomposeInProgress = false;
        private int _gdDecomposeDoneSteps = 0;
        private List<string> _gdDecomposeStepNames = new();
        private string _gdDecomposeCurrentStep = string.Empty;
        //
        private int _dataClsIdx = 0;
        private string _dataJson = string.Empty;
        //
        private bool _proposeEnergySystems = false;
        private bool _proposeQuestTypes = false;
        private bool _proposeAchievementTypes = false;
        private bool _proposeSocialMechanics = false;
        //
        private Vector2 _shortDescriptionScrollPosition;
        private string _shortDescription = string.Empty;
        //
        private string _newModelName = string.Empty;
        private string _newModelDescription = string.Empty;

        private enum TabIndices
        {
            GD_DECOMPOSE_IDX = 0,
            GEN_DATA_IDX,
        }

        private int _toolbarIdx = 0;
        private string[] _toolbarStrings = {
            "Game Design Proposals",
            "Generate Data",
        };
        //
        Vector2 _descriptionScrollPosition;

        [MenuItem("HyperEdge/CoDesigner/CoDesigner")]
        public static void ShowWindow()
        {
            var wnd = GetWindow(typeof(CoDesignerWindow));
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
                return;
            }
            if (_client is null)
            {
                _client = new HyperEdgeBackendClient();
            }
            //
            _appDef = AppDefCache.Instance.GetCurrentAppDef(_appData.Name);
            _py = new HyperEdgePy(_appData.Name);
            //
            if (_bag is null)
            {
                InitCallbacks();
            }
        }

        public void Destroy()
        {
            _bag?.Dispose();
        }

        private void InitCallbacks()
        {
            var d = DisposableBag.CreateBuilder();
            MessageHub.Instance.OnPyLlmGenDataDone.Subscribe(r =>
            {
                _dataJson = JsonConvert.SerializeObject(r.Data);
            }).AddTo(d);
            MessageHub.Instance.OnAppAIProposals.Subscribe(r =>
            {

            }).AddTo(d);
            //
            _bag = d.Build();
        }

        private void OnGUI()
        {
            if (_appDef is null)
            {
                EditorGUILayout.LabelField("Can't load any application definition", EditorStyles.boldLabel);
                return;
            }
            //
            _toolbarIdx = GUILayout.Toolbar(_toolbarIdx, _toolbarStrings);
            if (_toolbarIdx == (int)TabIndices.GD_DECOMPOSE_IDX)
            {
                OnGDDecomposeTab();
            }
            else if (_toolbarIdx == (int)TabIndices.GEN_DATA_IDX)
            {
                OnGenDataTab();
            }
        }

        private void OnGenDataTab()
        {
            // Display current data info
            //EditorGUILayout.LabelField("Current App:", EditorStyles.boldLabel);
            //
            var dataClassNames = _appDef.Data.DataClasses.Select(v => v.Name).ToArray();
            _dataClsIdx = EditorGUILayout.Popup(_dataClsIdx, dataClassNames);
            var dataCls = _appDef.Data.DataClasses[_dataClsIdx];
            //
            EditorGUILayout.TextField(_dataJson);
            //
            if (GUILayout.Button("Gen GameData!"))
            {
                _py.LlmGenData(dataCls.Name).Forget();
            }
        }
        
        private void OnGDDecomposeTab()
        {
            ProposeModelGUI();
            if (GUILayout.Button("Get All Proposals"))
            {
                FetchAIThreads().Forget();
            }
            //
            EditorGUILayout.LabelField("CoDesign options:", EditorStyles.boldLabel);
            //
            EditorGUILayout.LabelField("Proposals options:", EditorStyles.boldLabel);
            _proposeEnergySystems = EditorGUILayout.Toggle("EnergySystems", _proposeEnergySystems);
            _proposeAchievementTypes = EditorGUILayout.Toggle("Achievement Types", _proposeAchievementTypes);
            _proposeQuestTypes = EditorGUILayout.Toggle("Quest Types", _proposeQuestTypes); 
            _proposeSocialMechanics = EditorGUILayout.Toggle("Social Mechanics", _proposeSocialMechanics);
        
            if (_gdDecomposeInProgress && _gdDecomposeStepNames.Count > 0)
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Task Progress:", EditorStyles.boldLabel);
                if (!_GdSteps.TryGetValue(_gdDecomposeCurrentStep, out var currStepName))
                {
                    currStepName = "Processing AI Proposals";
                }
                //
                Rect pbRect = EditorGUILayout.BeginVertical();
                EditorGUI.ProgressBar(
                    pbRect,
                    (float)_gdDecomposeDoneSteps / _gdDecomposeStepNames.Count,
                    currStepName);
                GUILayout.Space(24);
                EditorGUILayout.EndVertical();
                //
                EditorGUILayout.EndVertical();
            }
        }

        private void ProposeModelGUI()
        {
            //_newModelName = EditorGUILayout.TextField(_newModelName);
            //
            /* Game short description */
            EditorGUILayout.LabelField("Short Description:", EditorStyles.boldLabel);
            _shortDescriptionScrollPosition = GUILayout.BeginScrollView(
                _shortDescriptionScrollPosition,
                GUILayout.Height(100));
            _shortDescription = EditorGUILayout.TextArea(
                _shortDescription,
                GUILayout.ExpandHeight(true));
            GUILayout.EndScrollView();
            //
            EditorGUILayout.LabelField("Game Description:", EditorStyles.boldLabel);
            _descriptionScrollPosition = GUILayout.BeginScrollView(
                _descriptionScrollPosition,
                GUILayout.Height(100));
            _newModelDescription = EditorGUILayout.TextArea(
                _newModelDescription,
                GUILayout.ExpandHeight(true));
            GUILayout.EndScrollView();
            //
            if (GUILayout.Button("Generate Data&Code"))
            {
                var req = new GDDecomposeRequest
                {
                    AppId = _appData.Id,
                    ShortDescription = _shortDescription,
                    Description = _newModelDescription,
                    //
                    ProposeEnergySystems = _proposeEnergySystems,
                    ProposeAcheivementTypes = _proposeAchievementTypes,
                    ProposeQuestTypes = _proposeQuestTypes,
                    ProposeSocialMechanics = _proposeSocialMechanics
                };
                _gdDecomposeDoneSteps = 0;
                DoGDDecompose(req).Forget();
                //_py.LlmProposeModel(_newModelName, _newModelDescription).Forget();
            }
        }

        private async UniTask DoGDDecompose(GDDecomposeRequest req)
        {
            _gdDecomposeInProgress = true;
            try
            {
                await DoGDDecomposeImpl(req);
            }
            finally
            {
                _gdDecomposeInProgress = false;
                _gdDecomposeCurrentStep = string.Empty;
                _gdDecomposeStepNames.Clear();
            }
        }

        private async UniTask DoGDDecomposeImpl(GDDecomposeRequest req)
        {
            var resp = await _client.GDDecompose(req);
            var jobInfo = await _client.GetJobById(resp.JobId);
            while (!jobInfo.Data.ContainsKey("steps"))
            {
                await UniTask.Delay(2000);
                jobInfo = await _client.GetJobById(resp.JobId);
            }
            _gdDecomposeStepNames = jobInfo.Data["steps"].ToObject<List<string>>(); 
            //
            while (!jobInfo.IsCompleted())
            {
                await UniTask.Delay(5000);
                jobInfo = await _client.GetJobById(resp.JobId);
                if (!jobInfo.Data.ContainsKey("current_step"))
                {
                    continue;
                }
                _gdDecomposeCurrentStep = jobInfo.Data["current_step"].ToObject<string>();
                //
                if (!jobInfo.Data.ContainsKey("progress"))
                {
                    continue;
                }
                var progressList = jobInfo.Data["progress"].ToObject<List<string>>();
                if (progressList.Count != _gdDecomposeDoneSteps)
                {
                    _gdDecomposeDoneSteps = progressList.Count;
                }
            }
        }

        private async UniTask FetchAIThreads()
        {
            var aiProposalsDirPath = $"Assets/HyperEdge/{_appData.Name}/AIProposals";
            if (!Directory.Exists(aiProposalsDirPath))
            {
                Directory.CreateDirectory(aiProposalsDirPath);
            }
            var client = new HyperEdgeBackendClient();
            var resp = await client.GetAppAIProposals(_appData.Id);
            for(int i = 0; i < resp.AIProposals.Count; i++)
            {
                var proposal = resp.AIProposals[i];
                var aiMsgs = await client.GetAIThread(proposal.ThreadId);
                var filename = $"{aiProposalsDirPath}/thread-{proposal.ThreadId}.json";
                var aiMsgsJson = JsonConvert.SerializeObject(aiMsgs);
                File.WriteAllText(filename, aiMsgsJson);

                if (EditorUtility.DisplayCancelableProgressBar(
                        "AIProposals",
                        $"Fetching AIThread Id={proposal.ThreadId}",
                        (float)(i+1)/resp.AIProposals.Count))
                {
                    break;
                }
            }
            //
            EditorUtility.ClearProgressBar();
        }
    }
}
