using UnityEngine;

namespace HyperEdge.Sdk.Unity.Flexi
{
    [CreateAssetMenu(fileName = "NewMacroAsset", menuName = "HyperEdge/Flexi/Macro Asset", order = 152)]
    public sealed class MacroAsset : GraphAsset
    {
        [SerializeField]
        [HideInInspector]
        private string text;

        internal string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
            }
        }
    }
}
