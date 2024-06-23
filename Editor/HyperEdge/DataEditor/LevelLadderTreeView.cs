using MessagePack;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

using HyperEdge.Shared.Protocol.Models;
using HyperEdge.Shared.Protocol.Models.Export;
using HyperEdge.Shared.Protocol.Models.Mechanics;


namespace HyperEdge.Sdk.Unity.DataEditor
{

public class LevelLadderTreeView : TreeView
{
    protected int _freeID = 0;
    protected readonly ProgressionSystemDTO _prog;
    protected readonly GenericLadderDTO _ladder;
    protected readonly AppDef _appDef;
    protected List<GenericLadderLevelDTO> _ladderLevels = new();
    private bool _hasAnyChanges = false;
    private bool _dirty = false;
    private byte[] _serializedLadder;

    public bool Dirty { get => _dirty; }

    public LevelLadderTreeView(
        AppDef appDef,
        TreeViewState state,
        MultiColumnHeader header,
        ProgressionSystemDTO prog,
        GenericLadderDTO ladder) : base(state, header)
    {
        _appDef = appDef;
        _freeID = 0;
        _prog = prog;
        _ladder = ladder;

        CloneInstances();

        showAlternatingRowBackgrounds = true;
        showBorder = true;
        cellMargin = 6;

        multiColumnHeader.sortingChanged += OnSortingChanged;
        multiColumnHeader.ResizeToFit();
    }

    private void CloneInstances()
    {
        _ladderLevels = _ladder.Clone().Levels;
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
        var newDi = new GenericLadderLevelDTO();
        //
        foreach (var fldDef in _prog.LadderLevelData.Fields)
        {
            var newDiFld = new DataClassInstanceFieldDTO();
            newDiFld.Name = fldDef.Name;
            newDiFld.Value = "";
            newDi.Data.Fields.Add(newDiFld);
        }
        _ladderLevels.Add(newDi);
        //
        var newItem = GenericLadderLevelTreeViewItem.Create(newDi, _ladderLevels.Count-1, this);

        rootItem.AddChild(newItem);
        rows.Add(newItem);

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
                    var itm = FindItem(idx, rootItem) as GenericLadderLevelTreeViewItem;
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
    }

    protected override void RowGUI(RowGUIArgs args)
    {
        var item = (GenericLadderLevelTreeViewItem)args.item;
        
        int cIdx = 0;
        Rect r = args.GetCellRect(cIdx);
        EditorGUI.LabelField(r, item.Level.ToString());
        cIdx++;
        
        if (_prog.IsExperienceBased)
        {
            r = args.GetCellRect(cIdx);
            var newColVal = EditorGUI.TextField(r, $"{item.DataItem.Exp}");
            cIdx++;
        }

        for (int i = 0; i < _prog.LadderLevelData.Fields.Count; ++i)
        {
            r = args.GetCellRect(cIdx);
            var fldDef = _prog.LadderLevelData.Fields[i];
            var colVal = item.DataItem.Data.Fields[i].Value;
            if (_appDef.DataClassesByName.TryGetValue(fldDef.Typename, out var nestedDataCls))
            {
                var instances = _appDef.GetDataClassInstancesByName(fldDef.Typename);
                var instNames = instances.Select(v => v.Name).ToArray();
                if (string.IsNullOrEmpty(colVal))
                {
                    colVal = $"{fldDef.Typename}/{instNames[0]}";
                    item.DataItem.Data.Fields[i].Value = colVal;
                    _hasAnyChanges = true;
                }
                var instNameParts = colVal.Split('/');
                var idx = instNames.ToList().FindIndex(v => v == instNameParts[1]);
                var newIdx = EditorGUI.Popup(r, idx, instNames);
                if (newIdx != idx)
                {
                    item.DataItem.Data.Fields[i].Value = $"{instNameParts[0]}/{instNames[newIdx]}";
                    _hasAnyChanges = true;
                }
            }
            else if (fldDef.Typename == "bool")
            {
                var prevColVal = colVal == "true";
                var newColVal = EditorGUI.Toggle(r, prevColVal);
                if (newColVal != prevColVal)
                {
                    item.DataItem.Data.Fields[i].Value = newColVal ? "true" : "false";
                    _hasAnyChanges = true;
                }
            }
            else
            {
                var newColVal = EditorGUI.TextField(r, "", colVal);
                if (newColVal != colVal)
                {
                    item.DataItem.Data.Fields[i].Value = newColVal;
                    _hasAnyChanges = true;
                }
            }
            cIdx++;
        }
    }

    protected override void BeforeRowsGUI()
    {
        _hasAnyChanges = false;
    }


    protected override void AfterRowsGUI()
    {
        if (_hasAnyChanges)
        {
            var serializedLadder = MessagePackSerializer.Serialize(_ladderLevels);
            _dirty = !serializedLadder.SequenceEqual(_serializedLadder);
            Debug.Log($"Dirty: {_dirty}");
        }
    }

    protected override TreeViewItem BuildRoot()
    {
        TreeViewItem root = new TreeViewItem();

        root.depth = -1;
        root.id = -1;
        root.parent = null;
        root.children = new List<TreeViewItem>();

        if (_ladderLevels is null)
        {
            return root;
        }

        for (int i = 0; i < _ladderLevels.Count; i++)
        {
            var ll = _ladderLevels[i];
            var child = GenericLadderLevelTreeViewItem.Create(ll, i, this);
            root.AddChild(child);
        }

        return root;
    }

    private List<GenericLadderLevelDTO> GetDiff()
    {
        List<GenericLadderLevelDTO> diff = new();
        for (int i = 0; i < _ladderLevels.Count; i++)
        {
            var ll = _ladderLevels[i];
            if (i < _ladder.Levels.Count)
            {
                var currLl = _ladder.Levels[i];
                var curBs = MessagePackSerializer.Serialize(currLl);
                var newBs = MessagePackSerializer.Serialize(ll);
                if(!curBs.SequenceEqual(newBs))
                {
                    diff.Add(ll);
                }
            }
            else
            {
                diff.Add(ll);
            }
        }
        return diff;
    }

    public void SaveChanges()
    {
        var prjPath = new HyperEdgePy(_appDef.Data.Name).GetPythonScriptsPath();
        var fname = $"{prjPath}/data/{_ladder.Name}.json";
        var diff = GetDiff();
        if (!File.Exists(fname))
        {
            var jData = JsonConvert.SerializeObject(diff);
            File.WriteAllText(fname, jData); 
        }
        else
        {
            var json = File.ReadAllText(fname);
            var jData = JsonConvert.SerializeObject(_ladder);
            File.WriteAllText(fname, jData); 
        }
    }
}

}

