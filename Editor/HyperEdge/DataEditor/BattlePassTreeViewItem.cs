using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

using HyperEdge.Shared.Protocol.Models;
using HyperEdge.Shared.Protocol.Models.Mechanics;


namespace HyperEdge.Sdk.Unity.DataEditor
{

public class BattlePassTreeViewItem : TreeViewItem
{
    public GenericLadderLevelDTO DataItem;
    public int Level = 0;

    public static BattlePassTreeViewItem Create(GenericLadderLevelDTO dataItem, int level, BattlePassTreeView treeView)
    {
        var newItem = new BattlePassTreeViewItem();
        newItem.children = new List<TreeViewItem>();
        newItem.depth = 0;
        newItem.id = treeView.GetNewID();
        newItem.Level = level;
        newItem.DataItem = dataItem;
        return newItem;
    }
}
}
