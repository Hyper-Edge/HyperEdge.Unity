using MessagePack;
using System;
using System.Collections.Generic;

using HyperEdge.Shared.Protocol.Models;
using HyperEdge.Shared.Protocol.Models.Mechanics;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class AddRulesResponse
    {
        public Ulid Id { get; set; }
        public RuleWorkflowsDTO Workflows { get; set; }
    }
}