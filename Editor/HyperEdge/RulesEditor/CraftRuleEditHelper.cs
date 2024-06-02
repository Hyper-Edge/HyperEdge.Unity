using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEditor;

using HyperEdge.Shared.Protocol.Models.Mechanics;
using HyperEdge.Shared.Protocol.Models.Export;


namespace HyperEdge.Sdk.Unity.RulesEditor
{
    public class CraftRuleEditHelper
    {
        private AppDef? _appDef = null;

        private string _craftRuleName = string.Empty;
        private RewardEditHelper _rewardHelper;
        private CostEditHelper _costHelper;

        public CraftRuleEditHelper(AppDef appDef)
        {
            _appDef = appDef;
            _rewardHelper = new RewardEditHelper(appDef);
            _costHelper = new CostEditHelper(appDef);
        }

        public void RenderGUI()
        {
            EditorGUILayout.BeginVertical();
            //
            EditorGUILayout.LabelField("Craft Rule Name:");
            _craftRuleName = EditorGUILayout.TextField(_craftRuleName);
            EditorGUILayout.Space();
            //
            EditorGUILayout.LabelField("Crafting reward:", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            _rewardHelper.RenderGUI(renderNameField: false);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Crafting cost:", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            _costHelper.RenderGUI();
            EditorGUILayout.EndVertical();
        }

        public CraftRulesDTO CreateRule()
        {
            var rule = new CraftRulesDTO
            {
                Name = _craftRuleName,
                Cost = _costHelper.Cost,
                Reward = _rewardHelper.Reward
            };
            return rule.Clone();
        }
    }
}

