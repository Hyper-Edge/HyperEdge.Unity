using System;
using System.Collections.Generic;
using MessagePack;

using HyperEdge.Shared.Protocol.Models.Export;


namespace HyperEdge.Shared.Protocol.Models.Mechanics
{

    [MessagePackObject(true)]
    public class BattlePassInstanceDTO
    {
        public Ulid Id { get; set; }
        public string Name { get; set; }
        public Ulid BattlePassId { get; set; }
        public string BattlePassName { get; set; }
        public List<DataClassInstanceFieldDTO> Fields { get; set; }
        public List<GenericLadderLevelDTO> Levels { get; set; }

        public BattlePassInstanceDTO Clone()
        {
            var bs = MessagePackSerializer.Serialize(this);
            return MessagePackSerializer.Deserialize<BattlePassInstanceDTO>(bs);
        }
    }

    [MessagePackObject(true)]
    public class BattlePassDTO
    {
        public Ulid Id { get; set; } 
        public string Name { get; set; }
        public DataClassFieldsDTO Model { get; set; } = new();
        public DataClassFieldsDTO Data { get; set; } = new();
        public DataClassFieldsDTO LadderLevelData { get; set; } = new();

        public BattlePassDTO Clone()
        {
            var bs = MessagePackSerializer.Serialize(this);
            return MessagePackSerializer.Deserialize<BattlePassDTO>(bs);
        }
    }
}
