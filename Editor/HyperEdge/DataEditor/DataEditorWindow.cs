using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Linq;

using HyperEdge.Shared.Protocol.Models.Export;


namespace HyperEdge.Sdk.Unity.DataEditor
{
public class DataEditorWindow : EditorWindow
{
    protected DatabaseTreeView _databaseDisplay;

    protected string[] _objectTypeNames;

    private AppData? _appData = null;
    private AppDef? _currentAppDef = null;

    private Dictionary<string, DatabaseTreeView> _treeViews = new();

    [SerializeField] protected int _selectedIndex = -1;

    private bool Dirty { get => _treeViews.Values.Any(v => v.Dirty); }

    [MenuItem("HyperEdge/DataEditor/Data")]
    static void Open()
    {
        GetWindow<DataEditorWindow>();
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
        _objectTypeNames = _currentAppDef.DataClasses.Select(v => v.Name).ToArray();

        if (_selectedIndex != -1 && _selectedIndex < _objectTypeNames.Length)
	    {
            CreateDisplayFrom(_currentAppDef.Data.DataClasses[_selectedIndex]);
	    }
    }

    void CreateDisplayFrom(DataClassDTO type)
    {
        if (type.Fields.Count == 0)
        {
            return;
        }
        //
        if (_treeViews.TryGetValue(type.Name, out var treeView))
        {
            _databaseDisplay = treeView;
        }
        else
        {
            TreeViewState state = new TreeViewState();
            MultiColumnHeaderState.Column[] columns = new MultiColumnHeaderState.Column[type.Fields.Count];
            //
            for (int i = 0; i < type.Fields.Count; ++i)
            {
                var fldData = type.Fields[i];
                columns[i] = new MultiColumnHeaderState.Column();
                columns[i].allowToggleVisibility = false;
                columns[i].headerContent = new GUIContent(fldData.Name);
                columns[i].minWidth = 64;// GetPropertyWidthFromType(prop.propertyType);
                columns[i].width = columns[i].minWidth;
                columns[i].canSort = CanSort(fldData.Typename);
            }
            //
            MultiColumnHeaderState headerstate = new MultiColumnHeaderState(columns);
            MultiColumnHeader header = new MultiColumnHeader(headerstate);
            //
            _databaseDisplay = new DatabaseTreeView(_currentAppDef, state, header, type);
            _treeViews[type.Name] = _databaseDisplay;
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
        int selected = EditorGUI.Popup(controlRect, "Type", _selectedIndex, _objectTypeNames);

        EditorGUIUtility.labelWidth = 0;

        if (EditorGUI.EndChangeCheck())
        {
            if (selected >= 0)
            {
                CreateDisplayFrom(_currentAppDef.Data.DataClasses[selected]);
                _selectedIndex = selected;
            }
        }

        if (_databaseDisplay != null)
        {
            if (_selectedIndex != -1)
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

