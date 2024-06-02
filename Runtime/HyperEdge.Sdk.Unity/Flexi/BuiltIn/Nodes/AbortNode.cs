namespace HyperEdge.Sdk.Unity.Flexi
{
    [NodeCategory(BuiltInCategory.Common)]
    public class AbortNode : ProcessNode
    {
        protected override AbilityState DoLogic()
        {
            return AbilityState.ABORT;
        }
    }
}
