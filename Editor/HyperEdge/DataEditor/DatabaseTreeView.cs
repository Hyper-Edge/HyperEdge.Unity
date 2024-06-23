using MessagePack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

using HyperEdge.Shared.Protocol.Models;
using HyperEdge.Shared.Protocol.Models.Export;
using HyperEdge.Shared.Protocol.Models.Mechanics;
using HyperEdge.Sdk.Unity.EditorHelpers;


namespace HyperEdge.Sdk.Unity.DataEditor
{

public class DatabaseTreeView : TreeView
{
    protected int _freeID = 0;
    protected readonly DataClassDTO _objectType;
    protected readonly AppDef _appDef;
    protected List<DataClassInstanceDTO> _instances = new();
    private bool _hasAnyChanges = false;
    private bool _dirty = false;
    private byte[] _serializedInsts;

    public bool Dirty { get => _dirty; }

    public DatabaseTreeView(AppDef appDef, TreeViewState state, MultiColumnHeader header, DataClassDTO objectType) : base(state, header)
    {
        _appDef = appDef;
        _freeID = 0;
        _objectType = objectType;

        CloneInstances();

        showAlternatingRowBackgrounds = true;
        showBorder = true;
        cellMargin = 6;

        multiColumnHeader.sortingChanged += OnSortingChanged;
        multiColumnHeader.ResizeToFit();
    }

    private void CloneInstances()
    {
        var instances = _appDef.GetDataClassInstancesByName(_objectType.Name);
        _serializedInsts = MessagePackSerializer.Serialize(instances);
        _instances = MessagePackSerializer.Deserialize<List<DataClassInstanceDTO>>(_serializedInsts);
    }

    void OnSortingChanged(MultiColumnHeader multiColumnHeader)
    {
        Sort(GetRows());
        Repaint();
    }

    public int GetNewID()
    {
        int id = _freeID;
        _freeID += 1;

        return id;
    }

    public void CreateNew()
    {
        var rows = GetRows();
        var newDi = new DataClassInstanceDTO();
        //
        foreach (var fldDef in _objectType.Fields)
        {
            var newDiFld = new DataClassInstanceFieldDTO();
            newDiFld.Name = fldDef.Name;
            newDiFld.Value = "";
            newDi.Fields.Add(newDiFld);
        }
        _instances.Add(newDi);
        //
        var newItem = DatabaseTreeViewItem.CreateFromDataItem(newDi, this);

        rootItem.AddChild(newItem);
        rows.Add(newItem);

        Sort(rows);

        Repaint();
    }

    protected override void KeyEvent()
    {
        if(Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Delete)
        {
            var list = GetSelection();
            if (EditorUtility.DisplayDialog("Confirm", "Confirm the suppression of the " + list.Count + " elected element?\nThis can't be undone.", "Yes", "No"))
            {
                var rows = GetRows();
                foreach (var idx in list)
                {
                    DatabaseTreeViewItem itm = FindItem(idx, rootItem) as DatabaseTreeViewItem;
                    rows.Remove(itm);
                    //AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(itm.obj.targetObject));
                }

                AssetDatabase.Refresh();
                Repaint();
            }

        }
        else
	    {
            base.KeyEvent();
	    }
    }

    void Sort(IList<TreeViewItem> rows)
    {
	/*
        if (multiColumnHeader.sortedColumnIndex == -1)
            return;

        if (rows.Count == 0)
            return;

        int sortedColumn = multiColumnHeader.sortedColumnIndex;
        var childrens = rootItem.children.Cast<DatabaseTreeViewItem>();

        var comparator = new SerializedPropertyComparator();
        var ordered = multiColumnHeader.IsSortedAscending(sortedColumn) ? childrens.OrderBy(k => k.properties[sortedColumn], comparator) : childrens.OrderByDescending(k => k.properties[sortedColumn], comparator);

        rows.Clear();
        foreach (var v in ordered)
            rows.Add(v);
	*/
    }

    protected override void RowGUI(RowGUIArgs args)
    {
        var item = (DatabaseTreeViewItem)args.item;
        for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
        {
            Rect r = args.GetCellRect(i);
            int column = args.GetColumn(i);
            var fldDef = _objectType.Fields[column];
            var colVal = item.DataItem.Fields[column].Value;
            if (_appDef.DataClassesByName.TryGetValue(fldDef.Typename, out var nestedDataCls))
            {
                var instances = _appDef.GetDataClassInstancesByName(fldDef.Typename);
                var instNames = instances.Select(v => v.Name).ToArray();
                if (string.IsNullOrEmpty(colVal))
                {
                    colVal = $"{fldDef.Typename}/{instNames[0]}";
                    item.DataItem.Fields[column].Value = colVal;
                    _hasAnyChanges = true;
                }
                var instNameParts = colVal.Split('/');
                var idx = instNames.ToList().FindIndex(v => v == instNameParts[1]);
                var newIdx = EditorGUI.Popup(r, idx, instNames);
                if (newIdx != idx)
                {
                    item.DataItem.Fields[column].Value = $"{instNameParts[0]}/{instNames[newIdx]}";
                    _hasAnyChanges = true;
                }
            }
            else if (fldDef.Typename == "Reward")
            {
                if (GUI.Button(r, "Edit"))
                {
                    var reward = string.IsNullOrEmpty(colVal) ? new RewardDTO() : JsonConvert.DeserializeObject<RewardDTO>(colVal);
                    var wnd = EditorHelper.OpenRewardEditor(reward, r => {
                        item.DataItem.Fields[column].Value = JsonConvert.SerializeObject(r);
                        _hasAnyChanges = true;
                    });
                }
            }
            else if (fldDef.Typename == "GenericExpLadder")
            {
                var ladderNames = _appDef.Data.ProgressionLadders.Select(el => $"{el.ProgressionName}{el.Name}").ToList();
                var menuItems = _appDef.Data.ProgressionLadders.Select(el => $"{el.ProgressionName}/{el.Name}").ToArray();
                var instNameParts = colVal.Split('/');
                var camelized = StringUtils.Camelize(instNameParts[1]);
                var idx = ladderNames.FindIndex(v => v == camelized);
                var newIdx = EditorGUI.Popup(r, idx, menuItems);
                if (idx != newIdx)
                {
                    var newInstName = StringUtils.Underscore(ladderNames[newIdx]);
                    item.DataItem.Fields[column].Value = $"GenericExpLadder/{newInstName}";
                    _hasAnyChanges = true;
                }
            }
            else if (fldDef.Typename == "bool")
            {
                var prevColVal = colVal == "true";
                var newColVal = EditorGUI.Toggle(r, prevColVal);
                if (newColVal != prevColVal)
                {
                    item.DataItem.Fields[column].Value = newColVal ? "true" : "false";
                    _hasAnyChanges = true;
                }
            }
            else
            {
                var newColVal = EditorGUI.TextField(r, "", colVal);
                if (newColVal != colVal)
                {
                    item.DataItem.Fields[column].Value = newColVal;
                    _hasAnyChanges = true;
                }
            }
        }
    }

    protected override void AfterRowsGUI()
    {
        if (_hasAnyChanges)
        {
            var serializedInsts = MessagePackSerializer.Serialize(_instances);
            _dirty = !serializedInsts.SequenceEqual(_serializedInsts);
            Debug.Log($"Dirty: {_dirty}");
            _hasAnyChanges = false;
        }
    }

    protected override TreeViewItem BuildRoot()
    {
        TreeViewItem root = new TreeViewItem();

        root.depth = -1;
        root.id = -1;
        root.parent = null;
        root.children = new List<TreeViewItem>();

        if (_instances is null)
        {
            return root;
        }

        foreach (var dataItem in _instances)
        {
            var child = DatabaseTreeViewItem.CreateFromDataItem(dataItem, this);
            root.AddChild(child);
        }

        return root;
    }

    private List<DataClassInstanceDTO> GetDiff()
    {
        List<DataClassInstanceDTO> diff = new();
        var curInsts = _appDef.GetDataClassInstancesByName(_objectType.Name)
            .ToDictionary(v => v.Name, v => v);
        foreach (var di in _instances)
        {
            if (curInsts.TryGetValue(di.Name, out var curDi))
            {
                var curBs = MessagePackSerializer.Serialize(curDi);
                var newBs = MessagePackSerializer.Serialize(di);
                if(!curBs.SequenceEqual(newBs))
                {
                    diff.Add(di);
                }
            }
            else
            {
                diff.Add(di);
            }
        }
        return diff;
    }

    public void SaveChanges()
    {
        var prjPath = new HyperEdgePy(_appDef.Data.Name).GetPythonScriptsPath();
        var fname = $"{prjPath}/data/{_objectType.Name}.json";
        var diff = GetDiff();
        if (!File.Exists(fname))
        {
            var jData = JsonConvert.SerializeObject(diff);
            File.WriteAllText(fname, jData); 
        }
        else
        {
            var json = File.ReadAllText(fname);
            var exDiff = JsonConvert.DeserializeObject<List<DataClassInstanceDTO>>(json);
            var exDiffByName = exDiff.ToDictionary(v => v.Name, v => v);
            foreach (var di in diff)
            {
                exDiffByName[di.Name] = di;
            }
            var jData = JsonConvert.SerializeObject(exDiffByName.Values.ToList());
            File.WriteAllText(fname, jData); 
        }
    }
}

}

