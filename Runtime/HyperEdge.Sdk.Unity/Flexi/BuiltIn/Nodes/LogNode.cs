//using UnityEngine;

namespace HyperEdge.Sdk.Unity.Flexi
{
    [NodeCategory(BuiltInCategory.Common)]
    public class LogNode : ProcessNode
    {
        public Inport<string> text;

        protected override AbilityState DoLogic()
        {
            //Debug.Log(text.GetValue());
            return AbilityState.RUNNING;
        }
    }
}
