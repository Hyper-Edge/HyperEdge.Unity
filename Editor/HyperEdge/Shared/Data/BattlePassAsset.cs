using System;
using UnityEngine;


namespace HyperEdge.Sdk.Unity
{
    [CreateAssetMenu(fileName = "NewBattlePassAsset", menuName = "HyperEdge/AppBuilder/BattlePass Asset", order = 100)]
    [Serializable]
    public class BattlePassAsset : HyperEdgeAsset
    {
        public string Name { get; set; }
    }
}
