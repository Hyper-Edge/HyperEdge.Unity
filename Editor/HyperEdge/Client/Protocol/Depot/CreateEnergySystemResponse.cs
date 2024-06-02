using System;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;
using HyperEdge.Shared.Protocol.Models.Mechanics;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class CreateEnergySystemResponse
    {
        public Ulid Id { get; set; }
        public EnergySystemDTO EnergySystem { get; set; }
    }
}
