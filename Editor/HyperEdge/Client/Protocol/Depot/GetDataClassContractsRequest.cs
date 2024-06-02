using System;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class GetDataClassContractsRequest
    {
        public Ulid AppId { get; set; }
        public Ulid OwnerId { get; set; }
    }
}
