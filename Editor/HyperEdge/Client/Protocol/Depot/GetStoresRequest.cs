using System;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class GetStoresRequest
    {
        public Ulid OwnerId { get; set; }
        public uint Page { get; set; }
    }
}
