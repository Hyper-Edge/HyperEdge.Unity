using System;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class AddGameDataRequest
    {
        public Ulid Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public DataClassItemDataDTO Data { get; set; }
    }
}
