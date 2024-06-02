using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;


namespace HyperEdge.Sdk.Unity
{
    public class GetAppAIProposalsResponse
    {
        public List<AIProposalDTO> AIProposals { get; set; } = new();
    }

    public class AIProposalDTO
    {
        public string Id { get; set; } = string.Empty;
        public string AppId { get; set; } = string.Empty;
        public string ThreadId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public JObject Data { get; set; } = new();
    }
}

