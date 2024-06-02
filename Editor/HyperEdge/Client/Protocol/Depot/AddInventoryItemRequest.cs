using System;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class AddInventoryItemRequest
    {
        public Ulid TokenId { get; set; }
        public Ulid DataClassItem { get; set; }
    }
}
