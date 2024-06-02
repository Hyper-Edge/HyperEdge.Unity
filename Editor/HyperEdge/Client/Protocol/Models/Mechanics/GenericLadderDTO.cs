using System;
using System.Collections.Generic;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol.Models.Mechanics
{
    [MessagePackObject(true)]
    public class GenericLadderLevelDTO
    {
        public ulong Exp { get; set; }
        public RewardDTO Reward { get; set; }
        public CostDTO Cost { get; set; }
        public List<string> Conditions;
        public DataClassItemDataDTO Data { get; set; }
    }

    [MessagePackObject(true)]
    public class GenericLadderDTO
    {
        public Ulid Id { get; set; }
        public Ulid ProgressionId { get; set; }
        public string ProgressionName { get; set; }
        public string Name { get; set; }
        public string LadderType { get; set; }
        public List<GenericLadderLevelDTO> Levels { get; set; }
    }
}
