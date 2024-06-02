using System;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class CreateDataClassItemRequest
    {
        public string Name { get; set; }
        public Ulid DataClassContract { get; set; }
        public DataClassItemDataDTO DataJson { get; set; }
    }
}
