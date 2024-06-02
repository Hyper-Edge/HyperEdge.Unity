using System;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class GetErc20TokenRequest
    {
        public Ulid Id { get; set; }
    }
}
