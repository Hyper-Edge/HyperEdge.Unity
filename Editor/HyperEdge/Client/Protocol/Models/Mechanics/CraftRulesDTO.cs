using System;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol.Models.Mechanics
{
    [MessagePackObject(true)]
    public class CraftRulesDTO : IGameData
    {
        public Ulid Id { get; set; }
        public string Name { get; set; }
        public CostDTO Cost { get; set; }
        public RewardDTO Reward { get; set; }

        public CraftRulesDTO Clone()
        {
            var bs = MessagePackSerializer.Serialize(this);
            return MessagePackSerializer.Deserialize<CraftRulesDTO>(bs);
        }
    }
}
