using System;
using System.Collections.Generic;


namespace HyperEdge.Sdk.Unity.Flexi
{
    public partial class NodeDeserializer
    {
        public static Node Deserialize(AbilityGraphNodeData nodeData)
        {
            var node = NodeDeserializer.CreateNode(nodeData.NodeType);
            node.id = nodeData.Id;
            return node;
        }
        
        public static Node CreateNode(string nodeTypeName)
        {
            var nodeType = ReflectionUtilities.GetTypeByName(nodeTypeName);
            if (nodeType is null)
            {
                return new MissingNode(nodeTypeName);
            }
            return NodeFactory.Create(nodeType);
        }
    }
}
