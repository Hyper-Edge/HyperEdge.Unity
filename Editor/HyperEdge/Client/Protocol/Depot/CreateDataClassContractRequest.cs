using MessagePack;
using System;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class CreateDataClassContractRequest
    {
        public Ulid AppId { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public DataClassContractDataDTO DataJson { get; set; }
    }
}
