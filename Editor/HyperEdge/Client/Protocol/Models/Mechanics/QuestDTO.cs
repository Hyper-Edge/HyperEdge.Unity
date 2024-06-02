using System;
using System.Collections.Generic;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol.Models.Mechanics
{
    [MessagePackObject(true)]
    public class QuestDTO
    {
        public Ulid Id { get; set; }
        public string Name { get; set; }
        public List<string> AcceptConditions { get; set; }
        public List<string> FinishConditions { get; set; }
        public DataClassContractDataDTO Model { get; set; }
        public DataClassContractDataDTO Data { get; set; }
    }
}
