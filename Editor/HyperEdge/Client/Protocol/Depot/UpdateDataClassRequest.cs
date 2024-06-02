using MessagePack;
using System;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class UpdateDataClassRequest
    {
        public Ulid Id { get; set; }
        public DataClassContractDataDTO DataJson { get; set; }
    }
}
