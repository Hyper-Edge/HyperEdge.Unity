using System;
using UnityEngine;


namespace HyperEdge.Sdk.Unity
{
    [CreateAssetMenu(fileName = "NewGameDataAsset", menuName = "HyperEdge/AppBuilder/GameData Asset", order = 100)]
    [Serializable]
    public class GameDataAsset : HyperEdgeAsset
    {
        public string Name;
	public string ScriptPath;
    }
}
