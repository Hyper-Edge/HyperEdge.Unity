using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using MessagePipe;
using Newtonsoft.Json;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEditor;

using HyperEdge.Shared.Protocol.Models.Export;


namespace HyperEdge.Sdk.Unity.CoDesigner
{
    public class ProposalEditorWindow : EditorWindow
    {
        private AppData? _appData = null;
        private HyperEdgePy? _py = null; 
        //
        private bool _loadingInProgress = false;
        private int _pIdx = -1;
        private ProposalsData _pData = new();
        private Dictionary<string, ProposalsData> _pDataCache = new();
        private Dictionary<string, AIProposalDTO> _exportedLlmProposals = new();
        private Dictionary<string, string> _pId2VersionId = new();

        private List<AIProposalDTO> _llmProposals = new();
        private string[] _llmProposalNames;
        // 
        private HyperEdgeBackendClient _client;
        private IDisposable _bag;        

        private int _actionIdx = 0;
        private string[] _actionNames = new string[] {
            "Edit",
            "Remove",
        };
        private enum EditorAction {
            EDIT,
            REMOVE
        };
        //
        private int _toolbarIdx = 0;
        private enum TabIndices
        {
            P_VIEW_IDX = 0,
            P_BUILD_IDX,
        };

        private string[] _toolbarStrings =
        {
            "View Proposals",
            "Build"
        };
        //
        enum LlmProposalType
        {
            GAME_MECHANICS = 0,
            USER_ACTIONS,
            DATA,
            MODEL,
            STRUCT,
            CURRENCY,
            RESOURCE,
            MINI_GAME
        };

        private int _llmProposalTypeIdx = -1;
        private static string[] _llmProposalTypes = new string[] {
            "Game Mechanics",
            "Player Actions",
            "",
            "Game Models",
            "",
            "Currency",
            "Resource",
            "Mini-Game"
        }; 

        [MenuItem("HyperEdge/CoDesigner/ProposalEditor")]
        public static void ShowWindow()
        {
            var wnd = GetWindow(typeof(ProposalEditorWindow));
            wnd.Show();
        }

        public void Awake()
        {
            if (_client is null)
            {
                _client = new HyperEdgeBackendClient();
            }
            if (_appData is null)
            {
                _appData = AppDataManager.Default.CurrentAppData;
            }
            if (_appData is null)
            {
                return;
            }
            _py = new HyperEdgePy(_appData.Name);
            //
            if (_bag is null)
            {
                var d = DisposableBag.CreateBuilder();
                MessageHub.Instance.OnAppAIProposals.Subscribe(r =>
                {
                    _llmProposals = r.AIProposals;
                    _llmProposalNames = _llmProposals.Select(el => $"p-{el.ThreadId}").ToArray();
                    _exportedLlmProposals.Clear();
                    _pId2VersionId.Clear();
                    foreach (var llmP in _llmProposals)
                    {
                        if (llmP.Data.ContainsKey("VersionId"))
                        {
                            var versionId = llmP.Data["VersionId"].ToObject<string>();
                            _exportedLlmProposals[versionId] = llmP;
                            _pId2VersionId[llmP.ThreadId] = versionId;
                        }
                    }
                }).AddTo(d);
                //
                MessageHub.Instance.OnPyLlmProposalExportDone.Subscribe(r =>
                {
                    Debug.Log($"Exported proposal VersionId={r.VersionId}");
                }).AddTo(d);
                //
                _bag = d.Build();
            }
            //
            _client.GetAppAIProposals(_appData.Id).Forget();
        }

        public void Destroy()
        {
            _bag.Dispose();
        }

        private void OnGUI()
        {
            if (_appData is null)
            {
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
            if (_llmProposalNames is null)
            {
                return;
            }
            if (_pIdx >= _llmProposals.Count)
            {
                return;
            }
            if (_loadingInProgress)
            {
                EditorGUILayout.LabelField("Loading proposal data...");
                return;
            }
            var pIdx = EditorGUILayout.Popup(_pIdx, _llmProposalNames);
            if (pIdx != _pIdx)
            {
                _pIdx = pIdx;
                var thId = _llmProposals[_pIdx].ThreadId;
                LoadProposalData(thId).Forget();
            }
            //
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Proposal Id:");
            EditorGUILayout.LabelField(_pData.Id);
            EditorGUILayout.EndHorizontal();
            //
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Version Id:");
            if (string.IsNullOrEmpty(_pData.VersionId))
            {
                EditorGUILayout.LabelField("Not exported");
            }
            else
            {
                EditorGUILayout.LabelField(_pData.VersionId);
            }
            EditorGUILayout.EndHorizontal();
            //
            _toolbarIdx = GUILayout.Toolbar(_toolbarIdx, _toolbarStrings);
            if (_toolbarIdx == (int)TabIndices.P_VIEW_IDX)
            {
                OnProposalsViewTab();
            }
            else if (_toolbarIdx == (int)TabIndices.P_BUILD_IDX)
            {
                OnProposalsBuildTab();
            }
        }

        private void OnProposalsViewTab()
        {
            _actionIdx = EditorGUILayout.Popup(_actionIdx, _actionNames);
            //
            _llmProposalTypeIdx = EditorGUILayout.Popup(_llmProposalTypeIdx, _llmProposalTypes);
            //
            if (_llmProposalTypeIdx == (int)LlmProposalType.GAME_MECHANICS)
            {
                OnGameMechanicsProposalTab();
            }
            else if (_llmProposalTypeIdx == (int)LlmProposalType.USER_ACTIONS)
            {
                OnUserActionsProposalTab();
            }
            else if (_llmProposalTypeIdx == (int)LlmProposalType.MODEL)
            {
                OnModelsProposalTab();
            }
            else if (_llmProposalTypeIdx == (int)LlmProposalType.CURRENCY)
            {
                OnCurrencyProposalTab();
            }
            else if (_llmProposalTypeIdx == (int)LlmProposalType.RESOURCE)
            {
                OnResourceProposalTab();
            }
            else if (_llmProposalTypeIdx == (int)LlmProposalType.MINI_GAME)
            {
                OnMiniGamesProposalTab();
            }
        }

        private void OnProposalsBuildTab()
        {
            if (GUILayout.Button("Export"))
            {
                ExportProposalImpl().Forget();
            }
            if (!string.IsNullOrEmpty(_pData.VersionId))
            {
                if (GUILayout.Button("Generate SDK code"))
                {
                    DoGenCodeImpl().Forget();
                }
                //
                if (GUILayout.Button("Build"))
                {
                    DoBuildAppVersionImpl().Forget();
                }
            }
        }

        private string GetProposalDataFilePath(string pId)
        {
            return $"Assets/HyperEdge/{_appData.Name}/AIProposals/thread-{pId}.json";
        }

        private async UniTask FetchProposalData(string pId)
        {
            var aiMsgs = await _client.GetAIThread(pId);
            var filename = GetProposalDataFilePath(pId);
            var aiMsgsJson = JsonConvert.SerializeObject(aiMsgs);
            File.WriteAllText(filename, aiMsgsJson);
        }

        private async UniTask LoadProposalData(string pId)
        {
            _loadingInProgress = true;
            if (_pDataCache.TryGetValue(pId, out var pData))
            {
                _pData = pData;
                return;
            }
            //
            var pInfoPath = GetProposalDataFilePath(pId);
            if (!File.Exists(pInfoPath))
            {
                await FetchProposalData(pId);
            }
            _pData = ProposalsData.LoadFromJsonFile(pInfoPath);
            _pData.Id = pId;
            if (_pId2VersionId.TryGetValue(pId, out var versionId))
            {
                _pData.VersionId = versionId;
            }
            _pDataCache[pId] = _pData;
            _loadingInProgress = false;
        }

        private void OnGameMechanicsProposalTab()
        {
        }

        private void OnUserActionsProposalTab()
        {
            EditorGUILayout.BeginVertical();
            foreach (var pInfo in _pData.UserActions)
            {
                UserActionProposalGUI(pInfo);
            }
            EditorGUILayout.EndVertical();
        }

        private void UserActionProposalGUI(LlmUserActionProposalInfo pInfo)
        {
            EditorGUILayout.BeginVertical();
            //
            EditorGUILayout.LabelField("Name:");
            EditorGUILayout.LabelField(pInfo.ActionName);
            //
            EditorGUILayout.EndVertical();
        }

        private void OnCurrencyProposalTab()
        {
            EditorGUILayout.BeginVertical();
            foreach (var pInfo in _pData.CurrencyProposals)
            {
                CurrencyProposalGUI(pInfo);
            }
            EditorGUILayout.EndVertical();
        }

        private void CurrencyProposalGUI(LlmCurrencyProposalInfo pInfo)
        {
            EditorGUILayout.BeginVertical();
            //
            EditorGUILayout.LabelField("Name:");
            EditorGUILayout.TextField(pInfo.Name);
            EditorGUILayout.LabelField("Description:");
            EditorGUILayout.TextArea(pInfo.Description);
            //
            EditorGUILayout.EndVertical();
        }

        private void OnResourceProposalTab()
        {
            EditorGUILayout.BeginVertical();
            foreach (var pInfo in _pData.ResourcesProposals)
            {
                ResourceProposalGUI(pInfo);
            }
            EditorGUILayout.EndVertical();
        }

        private void ResourceProposalGUI(LlmResourcesProposalInfo pInfo)
        {
            EditorGUILayout.BeginVertical();
            //
            EditorGUILayout.LabelField("Name:");
            EditorGUILayout.TextField(pInfo.Name);
            EditorGUILayout.LabelField("Description:");
            EditorGUILayout.TextArea(pInfo.Description);
            //
            EditorGUILayout.EndVertical();
        }

        private void OnMiniGamesProposalTab()
        {
            EditorGUILayout.BeginVertical();
            foreach (var pInfo in _pData.MiniGameProposals)
            {
                MiniGameProposalGUI(pInfo);
            }
            EditorGUILayout.EndVertical();
        }

        private void MiniGameProposalGUI(LlmMiniGameProposalInfo pInfo)
        {
            EditorGUILayout.BeginVertical();
            //
            EditorGUILayout.LabelField("Name:");
            EditorGUILayout.TextField(pInfo.Name);
            EditorGUILayout.LabelField("Description:");
            EditorGUILayout.TextArea(pInfo.Description);
            //
            EditorGUILayout.EndVertical();
        }

        private void OnModelsProposalTab()
        {
            EditorGUILayout.BeginVertical();
            foreach (var pInfo in _pData.ModelProposals)
            {
                ModelProposalGUI(pInfo);
            }
            EditorGUILayout.EndVertical();
        }

        private void ModelProposalGUI(LlmModelProposalInfo pInfo)
        {
            EditorGUILayout.BeginVertical();
            //
            EditorGUILayout.LabelField("Name:");
            EditorGUILayout.TextField(pInfo.Name);
            EditorGUILayout.LabelField("Description:");
            EditorGUILayout.TextArea(pInfo.Description);
            //
            EditorGUILayout.EndVertical();
        }

        private async UniTask<bool> ExportProposalImpl()
        {
            var pyRet = await _py.ConvertLlmThreadToAppDef(_pData.Id);
            if (!pyRet.IsSuccess)
            {
                EditorUtility.DisplayDialog("HyperEdge", "AIProposal export failed", "Ok");
                return false;
            }
            return true;
        }

        private async UniTask<bool> DoBuildAppVersionImpl()
        {
            const float BUILD_NSTEPS = 3.0f;
            var projectName = _appData.Name;
            //
            var versionData = _appData.Versions.Find(v => v.Id == _pData.VersionId);
            if (versionData is null)
            {
                EditorUtility.DisplayDialog("HyperEdge", $"Version {_pData.VersionId} doesn't exist", "Ok");
                return false;
            }
            //
            var progress = UnityEditor.Progress.Start(projectName,
                $"Building Server for ProposalId=\"{_pData.Id}\"");
            //
            var py = new HyperEdgePy(projectName);
            var pyRet = await py.BuildAppVersion(versionData.Name);
            if (!pyRet.IsSuccess)
            {
                EditorUtility.DisplayDialog("HyperEdge", "Build failed", "Ok");
                UnityEditor.Progress.Remove(progress);
                return false;
            }
            //
            EditorUtility.DisplayDialog("HyperEdge", $"Server build for AIProposal-{_pData.Id} Success", "Ok");
            return true;
        }

        private async UniTask<bool> DoGenCodeImpl()
        {
            const float GEN_CODE_NSTEPS = 3.0f;
            var projectName = _appData.Name;
            //
            var versionData = _appData.Versions.Find(v => v.Id == _pData.VersionId);
            if (versionData is null)
            {
                EditorUtility.DisplayDialog("HyperEdge", $"Version {_pData.VersionId} doesn't exist", "Ok");
                return false;
            }
            //
            var genCodeProgress = UnityEditor.Progress.Start(projectName,
                $"Generating SDK's for ProposalId=\"{_pData.Id}\"");
            //
            var py = new HyperEdgePy(projectName);
            var pyRet = await py.GenCodeAppVersion(versionData.Name);
            if (!pyRet.IsSuccess)
            {
                EditorUtility.DisplayDialog("HyperEdge", "Code generation failed", "Ok");
                UnityEditor.Progress.Remove(genCodeProgress);
                return false;
            }
            UnityEditor.Progress.Report(genCodeProgress, 1/GEN_CODE_NSTEPS, "Generated Code for SDK's");
            //
            var versionInfo = await _client.GetAppVersionInfo(versionData.Id);
            UnityEditor.Progress.Report(genCodeProgress, 2/GEN_CODE_NSTEPS, "Updated \"{versionData.Name}\" info");
            //
            if (versionInfo.Data.Files is null)
            {
                UnityEditor.Progress.Remove(genCodeProgress);
                return false;
            }
            //
            versionData.Files.SdkNodeFlowsFileId = versionInfo.Data.Files.SdkNodeFlowsFileId;
            versionData.Files.SdkSharedFileId = versionInfo.Data.Files.SdkSharedFileId;
            versionData.Files.SdkServerFileId = versionInfo.Data.Files.SdkServerFileId;
            EditorUtility.SetDirty(_appData);
            //
            await DoDownloadSdk(versionData);
            UnityEditor.Progress.Report(genCodeProgress, 3/GEN_CODE_NSTEPS, "Downloaded SDK components");
            //
            EditorUtility.DisplayDialog("HyperEdge", $"Code for AIProposal-{_pData.Id} downloaded", "Ok");
            //
            return true;
        }

        private async UniTask DoDownloadSdk(AppVersionInfo appVersionData)
        {
            var sdkComponents = new (string, string)[] {
                ( appVersionData.Files.SdkSharedFileId, "Sdk.Shared" ), 
                ( appVersionData.Files.SdkServerFileId, "Sdk.Server" ),
                ( appVersionData.Files.SdkNodeFlowsFileId, "Sdk.NodeFlows" ),
            };
            //
            for (int i = 0; i < sdkComponents.Length; i++)
            {
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
            var tgtDirname = $"Assets/Scripts/AIProposal-{_pData.Id}~/{_appData.Name}.{component}";
            await _client.DownloadFileById(fileId, tempFilename);
            if (Directory.Exists(tgtDirname))
            {
                Directory.Delete(tgtDirname, true);
            }
            ZipFile.ExtractToDirectory(tempFilename, tgtDirname);
        }
    }
}
