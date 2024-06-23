using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

using HyperEdge.Shared.Protocol.Models;
using HyperEdge.Shared.Protocol.Models.Mechanics;


namespace HyperEdge.Sdk.Unity.DataEditor
{

public class GenericLadderLevelTreeViewItem : TreeViewItem
{
    public GenericLadderLevelDTO DataItem;
    public int Level = 0;

    public static GenericLadderLevelTreeViewItem Create(GenericLadderLevelDTO dataItem, int level, LevelLadderTreeView treeView)
    {
        var newItem = new GenericLadderLevelTreeViewItem();
        newItem.children = new List<TreeViewItem>();
        newItem.depth = 0;
        newItem.id = treeView.GetNewID();
        newItem.Level = level;
        newItem.DataItem = dataItem;
        return newItem;
    }
}
}
