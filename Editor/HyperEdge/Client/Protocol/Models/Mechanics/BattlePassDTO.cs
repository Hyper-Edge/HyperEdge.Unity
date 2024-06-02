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
        public List<DataClassItemFieldDTO> Fields { get; set; }
        public List<GenericLadderLevelDTO> Levels { get; set; }
    }

    [MessagePackObject(true)]
    public class BattlePassDTO
    {
        public Ulid Id { get; set; } 
        public string Name { get; set; }
        public DataClassFieldsDTO Model { get; set; }
        public DataClassFieldsDTO Data { get; set; }
        public DataClassFieldsDTO LadderLevelData { get; set; }
    }
}
