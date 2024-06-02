using System;
using UnityEngine;


namespace HyperEdge.Sdk.Unity
{
    [CreateAssetMenu(fileName = "NewCraftingDataAsset", menuName = "HyperEdge/AppBuilder/Crafting Data Asset", order = 100)]
    [Serializable]
    public class CraftingDataAsset : HyperEdgeAsset
    {
        public string Name { get; set; }
    }
}
