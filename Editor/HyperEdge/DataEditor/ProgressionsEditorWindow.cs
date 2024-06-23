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
public class ProgressionsEditorWindow : EditorWindow
{
    protected LevelLadderTreeView _databaseDisplay;

    private string[] _progNames;
    private string[] _ladderNames = {};
    private List<GenericLadderDTO> _ladders = new();

    private AppData? _appData = null;
    private AppDef? _currentAppDef = null;

    private Dictionary<string, LevelLadderTreeView> _treeViews = new();

    protected int _progIdx = -1;
    protected int _ladderIdx = -1;

    private bool Dirty { get => _treeViews.Values.Any(v => v.Dirty); }

    [MenuItem("HyperEdge/DataEditor/Progressions")]
    static void Open()
    {
        GetWindow<ProgressionsEditorWindow>();
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
        _progNames = _currentAppDef.Data.Progressions.Select(v => v.EntityName).ToArray();

        if (_progIdx != -1 && _progIdx < _progNames.Length)
	    {
            var prog = _currentAppDef.Data.Progressions[_progIdx];
            _ladders = _currentAppDef.GetLaddersByProgression(prog);
            _ladderNames = _ladders.Select(el => el.Name).ToArray();
            if (_ladderIdx != -1 && _ladderIdx < _ladders.Count)
            {
                CreateDisplayFrom(prog, _ladders[_ladderIdx]);
            }
	    }
    }

    void CreateDisplayFrom(ProgressionSystemDTO prog, GenericLadderDTO ladder)
    {
        if (_treeViews.TryGetValue(prog.GetName(), out var treeView))
        {
            _databaseDisplay = treeView;
        }
        else
        {
            int fldCount = prog.LadderLevelData.Fields.Count + 1;
            if (prog.IsExperienceBased)
            {
                fldCount += 1;
            }
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
            if (prog.IsExperienceBased)
            {
                columns[fldIdx] = new MultiColumnHeaderState.Column();
                columns[fldIdx].allowToggleVisibility = false;
                columns[fldIdx].headerContent = new GUIContent("Exp");
                columns[fldIdx].minWidth = 8;// GetPropertyWidthFromType(prop.propertyType);
                columns[fldIdx].width = columns[fldIdx].minWidth;
                columns[fldIdx].canSort = false;
                fldIdx++;
            }
            for (int i = 0; i < prog.LadderLevelData.Fields.Count; ++i)
            {
                var fldData = prog.LadderLevelData.Fields[i];
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
            _databaseDisplay = new LevelLadderTreeView(_currentAppDef, state, header, prog, ladder);
            _treeViews[prog.GetName()] = _databaseDisplay;
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
        int progIdx = EditorGUI.Popup(controlRect, "Type", _progIdx, _progNames);
        controlRect.y += 20;
        int ladderIdx = EditorGUI.Popup(controlRect, "Instance", _ladderIdx, _ladderNames);

        EditorGUIUtility.labelWidth = 0;

        if (EditorGUI.EndChangeCheck())
        {
            if (progIdx >= 0 && _progIdx != progIdx)
            {
                var prog = _currentAppDef.Data.Progressions[progIdx];
                _ladders = _currentAppDef.GetLaddersByProgression(prog);
                _ladderNames = _ladders.Select(el => el.Name).ToArray();
                _progIdx = progIdx;
                _ladderIdx = 0;
                var ladder = _ladders[_ladderIdx];
                CreateDisplayFrom(prog, ladder);
            }
            //
            if (progIdx >= 0 && ladderIdx >= 0 && ladderIdx < _ladders.Count)
            {
                var prog = _currentAppDef.Data.Progressions[progIdx];
                var ladder = _ladders[ladderIdx];
                _ladderIdx = ladderIdx;
                CreateDisplayFrom(prog, ladder);
            }
        }

        if (_databaseDisplay != null)
        {
            if (_progIdx != -1)
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

