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
    public class CostEditHelper : ModelChooseHelper
    {
        private AppDef? _appDef = null;

        private CostDTO _newCost = new();
        private List<int> _selectedTypeIdxs = new();
        private List<int> _selectedDataInstIdxs = new();
	    private List<DataClassInstanceDTO> _newModelCostDataInsts = new();
        private string[] _newModelCostDataInstsNames;

        private int _newModelCostIdx = 0;
        private int _newModelCostDataInstIdx = 0;

        private Erc721Cost _newModelCost = new();
        private Erc1155Cost _newItemCost = new();

        public CostDTO Cost { get => _newCost; }

        public CostEditHelper(AppDef appDef) : base(appDef) 
        {
            _appDef = appDef;
        }

        public void SetCost(CostDTO cost)
        {
            _newCost = cost;
            _selectedTypeIdxs.Clear();
            foreach (var mCost in cost.Erc721Costs)
            {
                _selectedTypeIdxs.Add(GetModelTypeIdxByName(mCost.EntityName));
            }
        }

        private void RenderNewItemCostGUI()
        {
            EditorGUILayout.BeginHorizontal();
            //
            EditorGUILayout.LabelField("Add item", EditorStyles.boldLabel);
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
                _newItemCost.ItemId = $"{dataClsNames[_newItemTypeIdx]}/{diName}";
            }
            //
            var amountS = EditorGUILayout.TextField(_newItemCost.Amount.ToString());
            ulong amount = 0;
            if (ulong.TryParse(amountS, out amount))
            {
                _newItemCost.Amount = amount;
            }
            if (GUILayout.Button("+", GUILayout.Width(20)))
            {
                var newItemCost = new Erc1155Cost
                {
                    ItemId = _newItemCost.ItemId,
                    Amount = _newItemCost.Amount
                };
                _newCost.Erc1155Costs.Add(newItemCost);;
                _selectedItemTypeIdxs.Add(_newItemTypeIdx);
                _selectedItemDataInstIdxs.Add(_newItemDataInstIdx);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void RenderNewModelCostGUI()
        {
            EditorGUILayout.BeginHorizontal();
            //
            EditorGUILayout.LabelField("Add asset", EditorStyles.boldLabel);
            //
            var newModelTypeIdx = EditorGUILayout.Popup(_newModelCostIdx, ModelTypeNames);
            if (newModelTypeIdx != _newModelCostIdx || string.IsNullOrEmpty(_newModelCost.EntityName))
            {
                _newModelCostIdx = newModelTypeIdx;
                _newModelCost.EntityName = ModelTypeNames[newModelTypeIdx];
                _newModelCostDataInsts = _appDef.GetDataClassInstancesByModelName(_newModelCost.EntityName);
                if (_newModelCostDataInsts is not null)
                {
                    _newModelCostDataInstsNames = _newModelCostDataInsts.Select(v => v.Name).ToArray();
                }
            }
            //
            if (_newModelCostDataInsts is not null)
            {
                _newModelCostDataInstIdx = EditorGUILayout.Popup(_newModelCostDataInstIdx, _newModelCostDataInstsNames);
                _newModelCost.ItemId = _newModelCostDataInstsNames[_newModelCostDataInstIdx];
            }
            //
            var amountS = EditorGUILayout.TextField(_newModelCost.Amount.ToString());
            ulong amount = 0;
            if (ulong.TryParse(amountS, out amount))
            {
                _newModelCost.Amount = amount;
            }
            // Add attribute button
            if (GUILayout.Button("+", GUILayout.Width(20)))
            {
                var newModelCost = new Erc721Cost
                {
                    EntityName = _newModelCost.EntityName,
                    ItemId = _newModelCost.ItemId,
                    Amount = _newModelCost.Amount
                };
                _newCost.Erc721Costs.Add(newModelCost);
                _selectedTypeIdxs.Add(GetModelTypeIdxByName(_newModelCost.EntityName));
                _selectedDataInstIdxs.Add(_newModelCostDataInstIdx);
            }
            //
            EditorGUILayout.EndHorizontal();
        }

        public void RenderGUI()
        {
            RenderEditCostGUI();
            RenderNewModelCostGUI();
            RenderNewItemCostGUI();
        }

        private void RenderEditCostGUI()
        {
            EditorGUILayout.BeginVertical();
            //
            EditorGUILayout.LabelField("Model Costs:", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            for (int i = 0; i < _newCost.Erc721Costs.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{i+1}.", GUILayout.Width(40));
                var mCost = _newCost.Erc721Costs[i];
                var newTypeIdx = EditorGUILayout.Popup(_selectedTypeIdxs[i], ModelTypeNames);
                if (newTypeIdx != _selectedTypeIdxs[i])
                {
                    _selectedTypeIdxs[i] = newTypeIdx;
                    mCost.EntityName = ModelTypeNames[i];
                }
                //
	            var instances = _appDef.GetDataClassInstancesByModelName(mCost.EntityName);
                if (instances is not null)
                {
                    var diIdx = _selectedDataInstIdxs[i];
                    var diNames = instances.Select(v => v.Name).ToArray();
                    var newDiIdx = EditorGUILayout.Popup(diIdx, diNames);
                    if (newDiIdx != diIdx)
                    {
                        _selectedDataInstIdxs[i] = newDiIdx;
                        mCost.ItemId = diNames[newDiIdx];
                    }
                }

                var amountS = EditorGUILayout.TextField(mCost.Amount.ToString());
                ulong amount = 0;
                if (ulong.TryParse(amountS, out amount))
                {
                    mCost.Amount = amount;
                }
                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    _newCost.Erc721Costs.RemoveAt(i);
                    _selectedTypeIdxs.RemoveAt(i);
                    _selectedDataInstIdxs.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
            //
            var dataClsNames = _appDef.Data.DataClasses.Select(v => v.Name).ToArray();
            EditorGUILayout.LabelField("Item Costs:", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            for (int i = 0; i < _newCost.Erc1155Costs.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                var itemCost = _newCost.Erc1155Costs[i];
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
                    itemCost.ItemId = $"{dataClsNames[newTypeIdx]}/{diName}";
                }
                //
                var amountS = EditorGUILayout.TextField(itemCost.Amount.ToString());
                ulong amount = 0;
                if (ulong.TryParse(amountS, out amount))
                {
                    itemCost.Amount = amount;
                }
                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    _newCost.Erc1155Costs.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
            //
            EditorGUILayout.EndVertical();
        }
    }
}
