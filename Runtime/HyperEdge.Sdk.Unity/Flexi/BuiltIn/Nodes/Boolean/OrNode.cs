namespace HyperEdge.Sdk.Unity.Flexi
{
    [NodeCategory(BuiltInCategory.Logical)]
    public class OrNode : ValueNode
    {
        public Inport<bool> a;
        public Inport<bool> b;
        public Outport<bool> result;

        protected override void EvaluateSelf()
        {
            result.SetValue(a.GetValue() || b.GetValue());
        }
    }
}
