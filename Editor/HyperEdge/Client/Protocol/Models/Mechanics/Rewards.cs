using System;
using System.Collections.Generic;
using MessagePack;


namespace HyperEdge.Shared.Protocol.Models.Mechanics
{
    [MessagePackObject(true)]
    public class Erc721Reward
    {
        public string EntityName { get; set; }
        public string ItemId { get; set; }
        public ulong Amount { get; set; }
    }

    [MessagePackObject(true)]
    public class Erc1155Reward
    {
        public string ItemId { get; set; }
        public ulong Amount { get; set; }
    }

    [MessagePackObject(true)]
    public class RewardDTO : IGameData
    {
        public string Name { get; set; }
        public List<Erc721Reward> Erc721Rewards { get; set; } = new();
        public List<Erc1155Reward> Erc1155Rewards { get; set; } = new();

        public RewardDTO Clone()
        {
            var bs = MessagePackSerializer.Serialize(this);
            return MessagePackSerializer.Deserialize<RewardDTO>(bs);
        }
    }
}
