namespace HyperEdge.Sdk.Unity.Flexi
{
    internal sealed class StatRefreshEvent : IEventContext
    {
        // Empty Content
    }

    [NodeCategory(BuiltInCategory.Entry)]
    public class StatRefreshEventNode : EntryNode
    {
        public override bool CanExecute(IEventContext payloadObj)
        {
            return payloadObj is StatRefreshEvent;
        }
    }
}
