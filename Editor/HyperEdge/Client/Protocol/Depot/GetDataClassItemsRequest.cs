using System;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class GetDataClassItemsRequest
    {
        public Ulid DataClassContractId { get; set; }
        public uint Page { get; set; }
    }
}
