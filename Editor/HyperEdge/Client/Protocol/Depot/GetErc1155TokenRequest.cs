using System;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class GetErc1155TokenRequest
    {
        public Ulid Id { get; set; }
    }
}
