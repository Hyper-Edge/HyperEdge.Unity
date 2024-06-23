using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEditor;

using HyperEdge.Shared.Protocol.Models.Mechanics;
using HyperEdge.Shared.Protocol.Models.Export;
using HyperEdge.Sdk.Unity.EditorHelpers;


namespace HyperEdge.Sdk.Unity.RulesEditor
{
    public class RulesEditorWindow : EditorWindow
    {
        private AppData? _appData = null;
        private AppDef? _appDef = null;

        private RewardEditHelper _rewardHelper = null;
        private CostEditHelper _costHelper = null;
        private CraftRuleEditHelper _craftHelper = null;

        enum RuleType
        {
            CRAFT,
            REWARD,
        };

        private string[] _tabNames = new string[] {
            "Crafting",
            "Rewards",
        };
        private int _tabIdx = 0;

        [MenuItem("HyperEdge/RulesEditor")]
        public static void ShowWindow()
        {
            var wnd = GetWindow(typeof(RulesEditorWindow));
            wnd.Show();
        }

        private bool SetAppDef(AppDef? appDef)
        {
            _appDef = appDef;
            if (_appDef is null)
            {
                return false;
            }
            _rewardHelper = new RewardEditHelper(_appDef);
            _costHelper = new CostEditHelper(_appDef);
            _craftHelper = new CraftRuleEditHelper(_appDef);
            return true;
        }

        private void OnGUI()
        {
            if (_appData is null)
            {
                _appData = AppData.Load();
                if (_appData is null)
                {
                    return;
                }
            }
            //
            if (_appDef is null)
            {
                var appDef = AppDefCache.Instance.GetCurrentAppDef(_appData.Name);
                if (!SetAppDef(appDef))
                {
                    return;
                }
            }
            //
            _tabIdx = EditorGUILayout.Popup(_tabIdx, _tabNames);
            if (_tabIdx == (int)RuleType.CRAFT)
            {
                OnCraftingTab();
            }
            else if (_tabIdx == (int)RuleType.REWARD)
            {
                OnRewardsTab();
            }
        }

        private void OnCraftingTab()
        {
            _craftHelper.RenderGUI();
            //
            EditorGUILayout.Space();
            if (GUILayout.Button("Save"))
            {
                SaveCraftRule();
            }
        }

        private void OnRewardsTab()
        {
            _rewardHelper.RenderGUI(renderNameField: true);
            //
            EditorGUILayout.Space();
            if (GUILayout.Button("Save"))
            {
                SaveReward();
            }
        }

        private void UpdateJsonList<T>(string fname, T newData) where T : IGameData
        {
            var jText = File.ReadAllText(fname); 
            var jData = JsonConvert.DeserializeObject<List<T>>(jText);
            var dataByName = jData.ToDictionary(v => v.Name, v => v);
            dataByName[newData.Name] = newData;
            var newJText = JsonConvert.SerializeObject(dataByName.Values.ToList());
            File.WriteAllText(fname, newJText); 
        }

        private void SaveCraftRule()
        {
            var prjPath = HyperEdgePy.GetPythonScriptsPath(_appDef.Data.Name);
            var fname = $"{prjPath}/data/CraftRules.json";
            var newRule = _craftHelper.CreateRule();    

            if (!File.Exists(fname))
            {
                var tmpList = new List<CraftRulesDTO>();
                tmpList.Add(newRule);
                var jData = JsonConvert.SerializeObject(tmpList);
                File.WriteAllText(fname, jData); 
            }
            else
            {
                UpdateJsonList<CraftRulesDTO>(fname, newRule);
            }
        }

        private void SaveReward()
        {
            var prjPath = HyperEdgePy.GetPythonScriptsPath(_appDef.Data.Name);
            var fname = $"{prjPath}/data/Rewards.json";
            var newReward = _rewardHelper.CreateReward();
            //
            if (!File.Exists(fname))
            {
                var tmpList = new List<RewardDTO>();
                tmpList.Add(newReward);
                var jData = JsonConvert.SerializeObject(tmpList);
                File.WriteAllText(fname, jData); 
            }
            else
            {
                UpdateJsonList<RewardDTO>(fname, newReward);
            }
        }
    }
}
