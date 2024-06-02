using System;
using System.Collections.Generic;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class GetErc1155TokenResponse
    {
        public Erc1155TokenDTO Token { get; set; }
    }
}
