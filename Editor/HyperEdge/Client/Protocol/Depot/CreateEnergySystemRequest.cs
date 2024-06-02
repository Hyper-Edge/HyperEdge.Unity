using System;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class CreateEnergySystemRequest
    {
        public Ulid AppId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DataClassContractDataDTO Data { get; set; }
        public DataClassContractDataDTO Model { get; set; }
    }
}
