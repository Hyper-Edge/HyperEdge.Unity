using System;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class UpdateDataClassItemRequest
    {
        public Ulid Id { get; set; }
        public DataClassItemDataDTO DataJson { get; set; }
    }
}
