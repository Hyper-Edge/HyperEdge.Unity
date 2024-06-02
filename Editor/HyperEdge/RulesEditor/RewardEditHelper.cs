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
    public class RewardEditHelper : ModelChooseHelper
    {
        private AppDef? _appDef = null;

        private RewardDTO _newReward = new();
        private List<int> _selectedTypeIdxs = new();
        private List<int> _selectedDataInstIdxs = new();
        //
        private int _newRewardModelIdx = 0;
        private int _newModelDataInstIdx = 0; 
        private List<DataClassInstanceDTO> _newModelDataInsts = new();
        private string[] _newModelDataInstsNames;

        private Erc721Reward _newModelReward = new();
        private Erc1155Reward _newItemReward = new();

        public RewardDTO Reward { get => _newReward; }

        public RewardEditHelper(AppDef appDef) : base(appDef)
        {
            _appDef = appDef;
        }

        public void SetReward(RewardDTO reward)
        {
            _newReward = reward;
            _selectedTypeIdxs.Clear();
            foreach (var mReward in reward.Erc721Rewards)
            {
                _selectedTypeIdxs.Add(GetModelTypeIdxByName(mReward.EntityName));
            }
        }

        public RewardDTO CreateReward()
        {
            return _newReward.Clone();
        }

        private void RenderNewRewardGUI()
        {
            RenderNewModelRewardGUI();
            RenderNewItemRewardGUI();
        }

        private void RenderNewItemRewardGUI()
        {
            EditorGUILayout.LabelField("Add item", EditorStyles.boldLabel);
            //
            EditorGUILayout.BeginHorizontal();
            //
            var dataClsNames = _appDef.Data.DataClasses.Select(v => v.Name).ToArray();
            _newItemTypeIdx = EditorGUILayout.Popup(_newItemTypeIdx, dataClsNames);
            //
            var instances = _appDef.GetDataClassInstancesByName(dataClsNames[_newItemTypeIdx]);
            if (instances is not null && instances.Count > 0)
            {
                var instNames = instances.Select(v => v.Name).ToArray();
                _newItemDataInstIdx = EditorGUILayout.Popup(_newItemDataInstIdx, instNames);
                var diName = instNames[_newItemDataInstIdx];
                _newItemReward.ItemId = $"{dataClsNames[_newItemTypeIdx]}/{diName}";
            }
            //
            var amountS = EditorGUILayout.TextField(_newItemReward.Amount.ToString());
            ulong amount = 0;
            if (ulong.TryParse(amountS, out amount))
            {
                _newItemReward.Amount = amount;
            }
            if (GUILayout.Button("+", GUILayout.Width(20)))
            {
                var newItemReward = new Erc1155Reward
                {
                    ItemId = _newItemReward.ItemId,
                    Amount = _newItemReward.Amount
                };
                _newReward.Erc1155Rewards.Add(newItemReward);
                _selectedItemTypeIdxs.Add(_newItemTypeIdx);
                _selectedItemDataInstIdxs.Add(_newItemDataInstIdx);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void RenderNewModelRewardGUI()
        {
            EditorGUILayout.LabelField("Add asset", EditorStyles.boldLabel);
            //
            EditorGUILayout.BeginHorizontal();
            //
            var newModelTypeIdx = EditorGUILayout.Popup(_newRewardModelIdx, ModelTypeNames);
            if (newModelTypeIdx != _newRewardModelIdx || string.IsNullOrEmpty(_newModelReward.EntityName))
            {
                _newRewardModelIdx = newModelTypeIdx;
                _newModelReward.EntityName = ModelTypeNames[newModelTypeIdx];
                _newModelDataInsts = _appDef.GetDataClassInstancesByModelName(_newModelReward.EntityName);
                if (_newModelDataInsts is not null)
                {
                    _newModelDataInstsNames = _newModelDataInsts.Select(v => v.Name).ToArray();
                }
            }
            //
            if (_newModelDataInsts is not null && _newModelDataInsts.Count > 0)
            {
                _newModelDataInstIdx = EditorGUILayout.Popup(_newModelDataInstIdx, _newModelDataInstsNames);
                _newModelReward.ItemId = _newModelDataInstsNames[_newModelDataInstIdx];
            }
            //
            var amountS = EditorGUILayout.TextField(_newModelReward.Amount.ToString());
            ulong amount = 0;
            if (ulong.TryParse(amountS, out amount))
            {
                _newModelReward.Amount = amount;
            }
            // Add attribute button
            if (GUILayout.Button("+", GUILayout.Width(20)))
            {
                var newModelReward = new Erc721Reward
                {
                    EntityName = _newModelReward.EntityName,
                    ItemId = _newModelReward.ItemId,
                    Amount = _newModelReward.Amount
                };
                _newReward.Erc721Rewards.Add(newModelReward);
                _selectedTypeIdxs.Add(GetModelTypeIdxByName(_newModelReward.EntityName));
                _selectedDataInstIdxs.Add(_newModelDataInstIdx);
            }
            //
            EditorGUILayout.EndHorizontal();
        }

        public void RenderGUI(bool renderNameField)
        {
            EditorGUILayout.BeginVertical();
            //
            if (renderNameField)
            {
                EditorGUILayout.LabelField("Reward Name:");
                _newReward.Name = EditorGUILayout.TextField(_newReward.Name);
                EditorGUILayout.Space();
            }
            RenderEditRewardGUI();
            RenderNewRewardGUI();
        }

        private void RenderEditRewardGUI()
        {
            EditorGUILayout.LabelField("Model Rewards:");
            for (int i = 0; i < _newReward.Erc721Rewards.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                var mReward = _newReward.Erc721Rewards[i];
                var newTypeIdx = EditorGUILayout.Popup(_selectedTypeIdxs[i], ModelTypeNames);
                if (newTypeIdx != _selectedTypeIdxs[i])
                {
                    _selectedTypeIdxs[i] = newTypeIdx;
                    mReward.EntityName = ModelTypeNames[i];
                }
                //
	            var instances = _appDef.GetDataClassInstancesByModelName(mReward.EntityName);
                if (instances is not null)
                {
                    var diIdx = _selectedDataInstIdxs[i];
                    var diNames = instances.Select(v => v.Name).ToArray();
                    var newDiIdx = EditorGUILayout.Popup(diIdx, diNames);
                    if (newDiIdx != diIdx)
                    {
                        _selectedDataInstIdxs[i] = newDiIdx;
                        mReward.ItemId = diNames[newDiIdx];
                    }
                }
                //
                var amountS = EditorGUILayout.TextField(mReward.Amount.ToString());
                ulong amount = 0;
                if (ulong.TryParse(amountS, out amount))
                {
                    mReward.Amount = amount;
                }
                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    _newReward.Erc721Rewards.RemoveAt(i);
                    _selectedTypeIdxs.RemoveAt(i);
                    _selectedDataInstIdxs.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
            //
            var dataClsNames = _appDef.Data.DataClasses.Select(v => v.Name).ToArray();
            EditorGUILayout.LabelField("Item Rewards:");
            for (int i = 0; i < _newReward.Erc1155Rewards.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                var itemReward = _newReward.Erc1155Rewards[i];
                //
                var newTypeIdx = EditorGUILayout.Popup(_selectedItemTypeIdxs[i], dataClsNames);
                _selectedItemTypeIdxs[i] = newTypeIdx;
	            //
                var instances = _appDef.GetDataClassInstancesByName(dataClsNames[newTypeIdx]);
                if (instances is not null && instances.Count > 0)
                {
                    var instNames = instances.Select(v => v.Name).ToArray();
                    _selectedItemDataInstIdxs[i] = EditorGUILayout.Popup(_selectedItemDataInstIdxs[i], instNames);
                    var diName = instNames[_selectedItemDataInstIdxs[i]];
                    itemReward.ItemId = $"{dataClsNames[newTypeIdx]}/{diName}";
                }
                //
                var amountS = EditorGUILayout.TextField(itemReward.Amount.ToString());
                ulong amount = 0;
                if (ulong.TryParse(amountS, out amount))
                {
                    itemReward.Amount = amount;
                }
                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    _newReward.Erc1155Rewards.RemoveAt(i);
                    _selectedItemTypeIdxs.RemoveAt(i);
                    _selectedItemDataInstIdxs.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
            //
            EditorGUILayout.EndVertical();
        }
    }
}
