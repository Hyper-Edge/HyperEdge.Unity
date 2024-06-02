using System;
using System.Collections.Generic;


namespace HyperEdge.Sdk.Unity.Flexi
{
    public static class GraphDeserializer
    {
        public static AbilityGraph Deserialize(AbilityGraphData graphData)
        {
            var graph = new AbilityGraph();
            foreach (var nodeData in graphData.Nodes)
            {
                var node = NodeDeserializer.Deserialize(nodeData);
                graph.AddNode(node);
            }

            foreach (var edgeDto in graphData.Edges)
            {

                var srcNode = graph.GetNode(edgeDto.SrcNode);
                var srcPort = srcNode.GetPort(edgeDto.SrcPort);
                if (srcPort == null)
                {
                }

                var dstNode = graph.GetNode(edgeDto.DstNode);
                var dstPort = dstNode.GetPort(edgeDto.DstPort);
                if (dstPort == null)
                {
                }
                srcPort.Connect(dstPort);
            }

            return graph;
        }
    }
}

