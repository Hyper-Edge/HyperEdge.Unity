using System;
using MessagePack;

using HyperEdge.Shared.Protocol.Models.Export;


namespace HyperEdge.Shared.Protocol.Models.Mechanics
{
    [MessagePackObject(true)]
    public class EnergySystemDTO
    {
        public string Name { get; set; }
        public DataClassFieldsDTO Model { get; set; }
        public DataClassFieldsDTO Data { get; set; }
    }
}
