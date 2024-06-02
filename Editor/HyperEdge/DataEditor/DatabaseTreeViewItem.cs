using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

using HyperEdge.Shared.Protocol.Models;
using HyperEdge.Shared.Protocol.Models.Export;


namespace HyperEdge.Sdk.Unity.DataEditor
{

public class DatabaseTreeViewItem : TreeViewItem
{
    public DataClassInstanceDTO DataItem;

    public static DatabaseTreeViewItem CreateFromDataItem(DataClassInstanceDTO dataItem, DatabaseTreeView treeView)
    {
        DatabaseTreeViewItem newItem = new DatabaseTreeViewItem();
        newItem.children = new List<TreeViewItem>();
        newItem.depth = 0;
        newItem.id = treeView.GetNewID();
        newItem.DataItem = dataItem;
        return newItem;
    }
}
}
