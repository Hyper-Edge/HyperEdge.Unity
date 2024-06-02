using System;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class GetWeb3AppRequest
    {
        public Ulid Id { get; set; }
    }
}
