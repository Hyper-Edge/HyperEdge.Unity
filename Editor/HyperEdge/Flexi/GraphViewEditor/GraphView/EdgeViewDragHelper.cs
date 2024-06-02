using UnityEditor.Experimental.GraphView;

namespace HyperEdge.Sdk.Unity.Flexi.GraphViewEditor
{
    public class EdgeViewDragHelper<TEdgeView> : EdgeDragHelper<EdgeView> where TEdgeView : EdgeView, new()
    {
        public EdgeViewDragHelper(IEdgeConnectorListener listener) : base(listener)
        {

        }
    }
}
