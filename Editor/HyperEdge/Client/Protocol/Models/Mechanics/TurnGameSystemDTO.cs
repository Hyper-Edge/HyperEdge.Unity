using System;
using System.Collections.Generic;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;
using HyperEdge.Shared.Protocol.Models.Export;


namespace HyperEdge.Shared.Protocol.Models.Mechanics
{
    [MessagePackObject(true)]
    public class EntityFieldInitDTO
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Expr { get; set; }
    }

    [MessagePackObject(true)]
    public class GameEntityDescriptionDTO
    {
        public string Name { get; set; }
        public List<string> Stats { get; set; }
        public List<ContractFieldDTO> Fields { get; set; }
    }

    [MessagePackObject(true)]
    public class TurnGameSystemDTO
    {
        public Ulid Id { get; set; }
        public string Name { get; set; }
        //
        public List<DataClassDTO> GameEntitySlotDataTypes { get; set; }
        public List<DataClassDTO> GameEntityDataTypes { get; set; }
        public List<GameEntityDescriptionDTO> GameEntities { get; set; }
    } 
}
