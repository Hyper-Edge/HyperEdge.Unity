using System;
using System.Collections.Generic;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class GetErc721TokensResponse
    {
        public List<Erc721TokenDTO> Tokens;
        public long NumPages { get; set; }
        public long Total { get; set; }
    }
}
