using MessagePack;
using System;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class CreateErc1155TokenRequest
    {
        public string Name { get; set; }
        public Ulid AppId { get; set; }
        public string Data { get; set; }
    }
}
