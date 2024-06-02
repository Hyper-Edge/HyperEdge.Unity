using System;
using System.Collections.Generic;
using MessagePack;


namespace HyperEdge.Sdk.Unity.Flexi
{
    [MessagePackObject(true)]
    public class AbilityGraphEdgeData
    {
        public int SrcNode { get; set; }
        public string SrcPort { get; set; }
        public int DstNode { get; set; }
        public string DstPort { get; set; }
    }

    [MessagePackObject(true)]
    public class AbilityGraphNodeData
    {
        public int Id { get; set; }
        public string NodeType { get; set; }
    }

    [MessagePackObject(true)]
    public class AbilityGraphData
    {
        public Ulid Id { get; set; }
        public string Name { get; set; }
        public List<AbilityGraphNodeData> Nodes { get; set; }
        public List<AbilityGraphEdgeData> Edges { get; set; }
    }

    [MessagePackObject(true)]
    [Serializable]
    public class AbilityGraphGroup
    {
        public List<string> graphs = new List<string>();

        internal AbilityGraphGroup Clone()
        {
            var clone = new AbilityGraphGroup();
            for (var i = 0; i < graphs.Count; i++)
            {
                clone.graphs.Add(graphs[i]);
            }

            return clone;
        }
    }

    [MessagePackObject(true)]
    [Serializable]
    public class AbilityData
    {
        public string name;
        public List<BlackboardVariable> blackboard = new List<BlackboardVariable>();
        public List<AbilityGraphGroup> graphGroups = new List<AbilityGraphGroup>();

        /// <remarks>
        /// This method is for overriding values from outside data like Excel.
        /// </remarks>
        public void SetBlackboard(string key, int value)
        {
            for (var i = 0; i < blackboard.Count; i++)
            {
                if (blackboard[i].key == key)
                {
                    blackboard[i].value = value;
                    return;
                }
            }

            blackboard.Add(new BlackboardVariable { key = key, value = value });
        }

        public AbilityDataSource CreateDataSource(int groupIndex)
        {
            if (groupIndex < 0 || groupIndex >= graphGroups.Count)
            {
                throw new IndexOutOfRangeException();
            }

            return new AbilityDataSource(this, groupIndex);
        }
    }
}
