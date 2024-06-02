namespace HyperEdge.Sdk.Unity.Flexi
{
    [NodeCategory(BuiltInCategory.Entry)]
    public sealed class StartNode : EntryNode
    {
        public override bool CanExecute(IEventContext payloadObj)
        {
            return true;
        }
    }
}
