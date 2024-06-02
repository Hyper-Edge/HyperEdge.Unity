using System;
using System.Collections.Generic;


namespace HyperEdge.Sdk.Unity.CoDesigner
{
    public class LlmDataModelField
    {
        public string Name = string.Empty;
        public string Type = string.Empty;
        public string Description = string.Empty;
    }

    public class LlmDataModelProposalInfo
    {
        public string Name = string.Empty;
        public string Description = string.Empty;
        //
        public List<LlmDataModelField> Properties = new();
        public bool? IsResourceOrMaterial = false;
    }

    public class LlmModelProposalInfo
    {
        public string Name = string.Empty;
        public string Description = string.Empty;
        //
        public List<LlmDataModelField> MutableProperties = new();
        public List<LlmDataModelField> ImmutableProperties = new();
        public bool? IsResourceOrMaterial = false;
    }
}

