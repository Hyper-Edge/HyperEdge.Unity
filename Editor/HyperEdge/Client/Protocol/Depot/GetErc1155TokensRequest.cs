using System;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class GetErc1155TokensRequest
    {
        public uint Page { get; set; }
    }
}
