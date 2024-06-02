using System;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class CreateStoreRequest
    {
        public string Name { get; set; }
        public Ulid AppId { get; set; }
        public string Data { get; set; }
    }
}
