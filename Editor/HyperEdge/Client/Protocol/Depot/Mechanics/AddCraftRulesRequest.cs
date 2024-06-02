using System;
using System.Collections.Generic;
using MessagePack;


using HyperEdge.Shared.Protocol.Models;
using HyperEdge.Shared.Protocol.Models.Mechanics;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class AddCraftRulesRequest
    {
        public Ulid AppId { get; set; }
        public CraftRulesDTO CraftRules { get; set; }
    }
}
