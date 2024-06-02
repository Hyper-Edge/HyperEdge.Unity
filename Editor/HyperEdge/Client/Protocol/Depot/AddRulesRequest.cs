using MessagePack;
using System;
using System.Collections.Generic;

using HyperEdge.Shared.Protocol.Models;
using HyperEdge.Shared.Protocol.Models.Mechanics;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class RuleWorkflowsDTO
    {
        public string WorkflowsJson { get; set; }
    }

    [MessagePackObject(true)]
    public class AddRulesRequest
    {
        public Ulid AppId { get; set; }
        public RuleWorkflowsDTO Workflows { get; set; }
    }
}
