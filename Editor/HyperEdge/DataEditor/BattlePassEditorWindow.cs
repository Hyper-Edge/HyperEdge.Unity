using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Linq;

using HyperEdge.Shared.Protocol.Models.Export;
using HyperEdge.Shared.Protocol.Models.Mechanics;


namespace HyperEdge.Sdk.Unity.DataEditor
{
public class BattlePassEditorWindow : EditorWindow
{
    protected BattlePassTreeView _databaseDisplay;

    private string[] _battlePassNames;
    private string[] _instanceNames = {};
    private List<BattlePassInstanceDTO> _instances = new();

    private AppData? _appData = null;
    private AppDef? _currentAppDef = null;

    private Dictionary<string, BattlePassTreeView> _treeViews = new();

    protected int _bpIdx = -1;
    protected int _instanceIdx = -1;

    private bool Dirty { get => _treeViews.Values.Any(v => v.Dirty); }

    [MenuItem("HyperEdge/DataEditor/BattlePasses")]
    static void Open()
    {
        GetWindow<BattlePassEditorWindow>();
    }

    private void Awake()
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
        _databaseDisplay = null;
	    _currentAppDef = AppDefCache.Instance.GetCurrentAppDef(_appData.Name);
        _battlePassNames = _currentAppDef.Data.BattlePasses.Select(el => el.Name).ToArray();

        if (_bpIdx != -1 && _bpIdx < _battlePassNames.Length)
	    {
            var bp = _currentAppDef.Data.BattlePasses[_bpIdx];
            _instances = _currentAppDef.GetLaddersForBattlePass(bp);
            _instanceNames = _instances.Select(el => el.Name).ToArray();
            if (_instanceIdx != -1 && _instanceIdx < _instances.Count)
            {
                CreateDisplayFrom(bp, _instances[_instanceIdx]);
            }
	    }
    }

    void CreateDisplayFrom(BattlePassDTO bp, BattlePassInstanceDTO inst)
    {
        if (_treeViews.TryGetValue(inst.Name, out var treeView))
        {
            _databaseDisplay = treeView;
        }
        else
        {
            int fldCount = bp.LadderLevelData.Fields.Count + 2;
            TreeViewState state = new TreeViewState();
            MultiColumnHeaderState.Column[] columns = new MultiColumnHeaderState.Column[fldCount];
            //
            int fldIdx = 0;
            columns[0] = new MultiColumnHeaderState.Column();
            columns[0].allowToggleVisibility = false;
            columns[0].headerContent = new GUIContent("Level");
            columns[0].minWidth = 8;// GetPropertyWidthFromType(prop.propertyType);
            columns[0].width = columns[0].minWidth;
            columns[0].canSort = false;
            fldIdx++;
            //
            columns[fldIdx] = new MultiColumnHeaderState.Column();
            columns[fldIdx].allowToggleVisibility = false;
            columns[fldIdx].headerContent = new GUIContent("Exp");
            columns[fldIdx].minWidth = 8;// GetPropertyWidthFromType(prop.propertyType);
            columns[fldIdx].width = columns[fldIdx].minWidth;
            columns[fldIdx].canSort = false;
            fldIdx++;
            //
            for (int i = 0; i < bp.LadderLevelData.Fields.Count; ++i)
            {
                var fldData = bp.LadderLevelData.Fields[i];
                columns[fldIdx] = new MultiColumnHeaderState.Column();
                columns[fldIdx].allowToggleVisibility = false;
                columns[fldIdx].headerContent = new GUIContent(fldData.Name);
                columns[fldIdx].minWidth = 64;// GetPropertyWidthFromType(prop.propertyType);
                columns[fldIdx].width = columns[fldIdx].minWidth;
                columns[fldIdx].canSort = CanSort(fldData.Typename);
                fldIdx++;
            }
            //
            MultiColumnHeaderState headerstate = new MultiColumnHeaderState(columns);
            MultiColumnHeader header = new MultiColumnHeader(headerstate);
            //
            _databaseDisplay = new BattlePassTreeView(_currentAppDef, state, header, bp, inst);
            _treeViews[inst.Name] = _databaseDisplay;
        }
        _databaseDisplay.Reload();
    }
    
    bool CanSort(string typeName)
    {
        return false;
    }

    private void OnGUI()
    {
        if (_appData is null || _currentAppDef is null)
        {
            EditorGUILayout.LabelField("Can't load any application definition", EditorStyles.boldLabel);
            return;
        }
        EditorGUILayout.BeginVertical();
        //
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
        EditorGUI.BeginChangeCheck();

        EditorGUIUtility.labelWidth = 40;
        Rect controlRect = new Rect(0, 80, 200, EditorGUIUtility.singleLineHeight);
        int bpIdx = EditorGUI.Popup(controlRect, "Type", _bpIdx, _battlePassNames);
        controlRect.y += 20;
        int instanceIdx = EditorGUI.Popup(controlRect, "Instance", _instanceIdx, _instanceNames);

        EditorGUIUtility.labelWidth = 0;

        if (EditorGUI.EndChangeCheck())
        {
            if (bpIdx >= 0 && _bpIdx != bpIdx)
            {
                var bp = _currentAppDef.Data.BattlePasses[bpIdx];
                _instances = _currentAppDef.GetLaddersForBattlePass(bp);
                _instanceNames = _instances.Select(el => el.Name).ToArray();
                _bpIdx = bpIdx;
                _instanceIdx = 0;
                var inst = _instances[_instanceIdx];
                CreateDisplayFrom(bp, inst);
            }
            //
            if (bpIdx >= 0 && instanceIdx >= 0 && instanceIdx < _instances.Count)
            {
                var bp = _currentAppDef.Data.BattlePasses[bpIdx];
                var inst = _instances[instanceIdx];
                _instanceIdx = instanceIdx;
                CreateDisplayFrom(bp, inst);
            }
        }

        if (_databaseDisplay != null)
        {
            if (_bpIdx != -1)
            {
                controlRect.y += controlRect.height;
                controlRect.x = 0;
                controlRect.width = 64;

                if (GUI.Button(controlRect, "New"))
                {
                    _databaseDisplay.CreateNew();
                }

                controlRect.x += controlRect.width + 12;
                controlRect.width = 100;
                //
                if (Dirty)
                {
                    if (GUI.Button(controlRect, "Save Changes"))
                    {
                        SaveChanges();
                    }
                }
            }

            float startY = controlRect.y + controlRect.height + 12;
            Rect r = new Rect(0, startY, position.width, position.height - startY);

            _databaseDisplay.OnGUI(r);
        }
        //
        EditorGUILayout.EndVertical();
    }

    public void SaveChanges()
    {
        foreach (var treeView in _treeViews.Values)
        {
            if (treeView.Dirty)
            {
                treeView.SaveChanges();
            }
        }
    }
}
}

