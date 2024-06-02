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
    public class ModelChooseHelper
    {
        private AppDef? _appDef = null;
        private string[] _availableTypeNames;
        private Dictionary<string, int> _typeName2Idx = new();
        //
        protected int _newItemTypeIdx = 0;
        protected int _newItemDataInstIdx = 0;
        //
        protected List<int> _selectedItemTypeIdxs = new();
        protected List<int> _selectedItemDataInstIdxs = new();
        //
        public string[] ModelTypeNames { get => _availableTypeNames; }
        
        public ModelChooseHelper(AppDef appDef)
        {
            _appDef = appDef;
            LoadModelNames();
        }

        void LoadModelNames()
        {
            _availableTypeNames = _appDef.Data.ModelClasses.Select(m => m.Name).ToArray();
            for (int i = 0; i < _availableTypeNames.Length; i++)
            {
                _typeName2Idx[_availableTypeNames[i]] = i;
            }
        }

        public int GetModelTypeIdxByName(string typeName)
        {
            return _typeName2Idx[typeName];
        }

        public string GetModelTypenameByIdx(int idx)
        {
            return _availableTypeNames[idx];
        }
    }
}
