using System;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class GetInventoryItemsRequest
    {
        public Ulid AppId { get; set; }
    }
}
