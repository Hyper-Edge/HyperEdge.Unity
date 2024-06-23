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
using HyperEdge.Sdk.Unity.EditorHelpers;


namespace HyperEdge.Sdk.Unity.DataEditor
{

public class BattlePassTreeView : TreeView
{
    protected int _freeID = 0;
    protected readonly BattlePassDTO _bp;
    protected readonly BattlePassInstanceDTO _inst;
    protected readonly AppDef _appDef;
    protected List<GenericLadderLevelDTO> _instLevels = new();
    private bool _hasAnyChanges = false;
    private bool _dirty = false;
    private byte[] _serializedLadder;

    public bool Dirty { get => _dirty; }

    public BattlePassTreeView(
        AppDef appDef,
        TreeViewState state,
        MultiColumnHeader header,
        BattlePassDTO bp,
        BattlePassInstanceDTO inst) : base(state, header)
    {
        _appDef = appDef;
        _freeID = 0;
        _bp = bp;
        _inst = inst;

        CloneInstances();

        showAlternatingRowBackgrounds = true;
        showBorder = true;
        cellMargin = 6;

        multiColumnHeader.sortingChanged += OnSortingChanged;
        multiColumnHeader.ResizeToFit();
    }

    private void CloneInstances()
    {
        _instLevels = _inst.Clone().Levels;
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
        foreach (var fldDef in _bp.LadderLevelData.Fields)
        {
            var newDiFld = new DataClassInstanceFieldDTO();
            newDiFld.Name = fldDef.Name;
            newDiFld.Value = "";
            newDi.Data.Fields.Add(newDiFld);
        }
        _instLevels.Add(newDi);
        //
        var newItem = BattlePassTreeViewItem.Create(newDi, _instLevels.Count-1, this);

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
        var item = (BattlePassTreeViewItem)args.item;
        
        int cIdx = 0;
        Rect r = args.GetCellRect(cIdx);
        EditorGUI.LabelField(r, item.Level.ToString());
        cIdx++;
        //
        r = args.GetCellRect(cIdx);
        var newExpVal = EditorGUI.TextField(r, $"{item.DataItem.Exp}");
        cIdx++;
        //
        r = args.GetCellRect(cIdx);
        if (GUI.Button(r, "Edit"))
        {
            var rewardEditWnd = EditorWindow.GetWindow(typeof(RewardEditWindow), utility: true) as RewardEditWindow;
            rewardEditWnd.SetReward(item.DataItem.Reward);
            rewardEditWnd.Show();
        }
        cIdx++;
        //
        for (int i = 0; i < _bp.LadderLevelData.Fields.Count; ++i)
        {
            r = args.GetCellRect(cIdx);
            var fldDef = _bp.LadderLevelData.Fields[i];
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
            var serializedLadder = MessagePackSerializer.Serialize(_instLevels);
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

        if (_instLevels is null)
        {
            return root;
        }

        for (int i = 0; i < _instLevels.Count; i++)
        {
            var ll = _instLevels[i];
            var child = BattlePassTreeViewItem.Create(ll, i, this);
            root.AddChild(child);
        }

        return root;
    }

    private List<GenericLadderLevelDTO> GetDiff()
    {
        List<GenericLadderLevelDTO> diff = new();
        for (int i = 0; i < _instLevels.Count; i++)
        {
            var ll = _instLevels[i];
            if (i < _inst.Levels.Count)
            {
                var currLl = _inst.Levels[i];
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
        var fname = $"{prjPath}/data/{_inst.Name}.json";
        var diff = GetDiff();
        if (!File.Exists(fname))
        {
            var jData = JsonConvert.SerializeObject(diff);
            File.WriteAllText(fname, jData); 
        }
        else
        {
            var json = File.ReadAllText(fname);
            var jData = JsonConvert.SerializeObject(_inst);
            File.WriteAllText(fname, jData); 
        }
    }
}

}

