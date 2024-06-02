using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEditor;

using HyperEdge.Shared.Protocol.Models.Export;


namespace HyperEdge.Sdk.Unity.EntityEditor
{
    public class DataClassEditHelper
    {
        private AppDef? _appDef = null;
        private DataClassDTO? _dataClass = null;
        //
        private List<int> _selectedTypeIdxs = new();
        private string[] _availableTypeNames;
        private Dictionary<string, int> _typeName2Idx = new();
        //
        private string _newFieldName = "";
        private string _newFieldValue = "";
        private string _newFieldDataType = "";
        private int _newFieldDataTypeIdx = 0;
        private bool _newFieldDataTypeChanged = false;

        private string[] _primitive_types = new string[] {
            "bool",
            "string",
            "int",
            "UInt32",
            "UInt64",
            "Int32",
            "Int64",
            "Float",
            "Ulid",
            //
            "Reward",
            "GenericLadder",
            "GenericExpLadder",
        };
        
        public DataClassEditHelper(AppDef appDef, DataClassDTO? dataClass)
        {
            _appDef = appDef;
            _dataClass = dataClass;
            LoadAvailableTypes();
            LoadExistingClass();
        }

        public void SetDataClass(DataClassDTO dataClass)
        {
            if (_dataClass == dataClass)
            {
                return;
            }

            _dataClass = dataClass;
            _newFieldName = "";
            _newFieldValue = "";
            _newFieldDataType = "";
            _newFieldDataTypeIdx = 0;

            _selectedTypeIdxs.Clear();
            LoadExistingClass();
        }

        private void LoadAvailableTypes()
        {
            var l = _primitive_types.ToList();
            //
            foreach (var stData in _appDef.Data.UGCDataClasses)
            {
                l.Add(stData.Name);
            }
            //
            foreach (var stData in _appDef.Data.DataClasses)
            {
                l.Add(stData.Name);
            }
            //
            foreach (var stData in _appDef.Data.StructClasses)
            {
                l.Add(stData.Name);
            }
            _availableTypeNames = l.ToArray();
            for (int i = 0; i < _availableTypeNames.Length; i++)
            {
                _typeName2Idx[_availableTypeNames[i]] = i;
            }
        }

        private void LoadExistingClass()
        {
            if (_dataClass is null)
            {
                return; 
            }
            foreach (var fldData in _dataClass.Fields)
            {
                _selectedTypeIdxs.Add(_typeName2Idx[fldData.Typename]);
            }
        }

        public void RenderDataClassEditGUI()
        {
            EditorGUILayout.LabelField("Class Name:");
            _dataClass.Name = EditorGUILayout.TextField(_dataClass.Name);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Fields:");
            // Display existing _dataClass.Fields
            for (int i = 0; i < _dataClass.Fields.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                // Attribute name
                _dataClass.Fields[i].Name = EditorGUILayout.TextField(_dataClass.Fields[i].Name);
                // Attribute data type
                _selectedTypeIdxs[i] = EditorGUILayout.Popup(_selectedTypeIdxs[i], _availableTypeNames);
                _dataClass.Fields[i].Typename = _availableTypeNames[_selectedTypeIdxs[i]];
                // Attribute value
                switch (_dataClass.Fields[i].Typename)
                {
                    case "string":
                        if (_dataClass.Fields[i].DefaultValue == "False" || _dataClass.Fields[i].DefaultValue == "True" || _dataClass.Fields[i].DefaultValue == "0")
                        {
                            _dataClass.Fields[i].DefaultValue = ""; // Clear the value if it's "False" or "True"
                        }
                        _dataClass.Fields[i].DefaultValue = EditorGUILayout.TextField(_dataClass.Fields[i].DefaultValue);
                        break;
                    case "int":
                    case "Int32":
                    case "Int64":
                        int intValue;
                        if (int.TryParse(_dataClass.Fields[i].DefaultValue, out intValue))
                            _dataClass.Fields[i].DefaultValue = EditorGUILayout.IntField(intValue).ToString();
                        else
                            _dataClass.Fields[i].DefaultValue = EditorGUILayout.IntField(0).ToString();
                        break;
                    case "Float":
                        float floatValue;
                        if (float.TryParse(_dataClass.Fields[i].DefaultValue, out floatValue))
                            _dataClass.Fields[i].DefaultValue = EditorGUILayout.FloatField(floatValue).ToString();
                        else
                            _dataClass.Fields[i].DefaultValue = EditorGUILayout.FloatField(0.0f).ToString();
                        break;
                    case "Boolean":
                        bool boolValue;
                        if (bool.TryParse(_dataClass.Fields[i].DefaultValue, out boolValue))
                            _dataClass.Fields[i].DefaultValue = EditorGUILayout.Toggle(boolValue).ToString();
                        else
                            _dataClass.Fields[i].DefaultValue = EditorGUILayout.Toggle(false).ToString();
                        break;
                }
                // Remove attribute button
                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    _dataClass.Fields.RemoveAt(i);
                    _selectedTypeIdxs.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.Space();
            //
            EditorGUILayout.BeginHorizontal();
            // Attribute name
            _newFieldName = EditorGUILayout.TextField(_newFieldName);

            // Attribute data type
            _newFieldDataTypeIdx = EditorGUILayout.Popup(_newFieldDataTypeIdx, _availableTypeNames);
            var newDataType = _availableTypeNames[_newFieldDataTypeIdx];
            if (newDataType != _newFieldDataType)
            {
                _newFieldDataType = newDataType;
                _newFieldDataTypeChanged = true;
            }

            // Attribute value
            switch (_newFieldDataType)
            {
                case "string":
                    if (_newFieldValue == "False" || _newFieldValue == "True" || _newFieldValue == "0")
                    {
                        _newFieldValue = ""; // Clear the value if it's "False" or "True"
                    }
                    _newFieldValue = EditorGUILayout.TextField(_newFieldValue);
                    break;
                case "int":
                case "Int32":
                case "Int64":
                    int intValue;
                    if (int.TryParse(_newFieldValue, out intValue))
                        _newFieldValue = EditorGUILayout.IntField(intValue).ToString();
                    else
                        _newFieldValue = EditorGUILayout.IntField(0).ToString();
                    break;
                case "Float":
                    float floatValue;
                    if (float.TryParse(_newFieldValue, out floatValue))
                        _newFieldValue = EditorGUILayout.FloatField(floatValue).ToString();
                    else
                        _newFieldValue = EditorGUILayout.FloatField(0.0f).ToString();
                    break;
                case "bool":
                    bool boolValue;
                    if (bool.TryParse(_newFieldValue, out boolValue))
                        _newFieldValue = EditorGUILayout.Toggle(boolValue).ToString();
                    else
                        _newFieldValue = EditorGUILayout.Toggle(false).ToString();
                    break;
            }

            // Add attribute button
            if (GUILayout.Button("+", GUILayout.Width(20)))
            {
                if (_newFieldDataTypeChanged)
                {
                    switch (_newFieldDataType)
                    {
                        case "string":
                            _newFieldValue = "";
                            break;
                        case "int":
                        case "Int32":
                        case "Int64":
                            _newFieldValue = "0";
                            break;
                        case "Float":
                            _newFieldValue = "0.0";
                            break;
                        case "bool":
                            _newFieldValue = "False";
                            break;
                    }
                    _newFieldDataTypeChanged = false;
                }
                _dataClass.Fields.Add(new DataClassFieldDTO {
                    Name = _newFieldName,
                    Typename = _newFieldDataType,
                    DefaultValue =_newFieldValue
                });
                _selectedTypeIdxs.Add(_typeName2Idx[_newFieldDataType]);
                _newFieldName = "";
                _newFieldValue = "";
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
