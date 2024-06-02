using UnityEngine;


namespace HyperEdge.Sdk.Unity
{
    [CreateAssetMenu(fileName = "NewGameModelAsset", menuName = "HyperEdge/AppBuilder/GameModel Asset", order = 100)]
    public class GameModelAsset : HyperEdgeAsset
    {
        public string Name { get; set; }
    }
}
