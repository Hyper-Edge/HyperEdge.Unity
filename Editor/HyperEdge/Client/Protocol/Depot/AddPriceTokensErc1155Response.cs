using System;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class AddPriceErc1155TokensResponse
    {
        public PriceErc1155TokensDTO PriceTokens { get; set; }
    }
}
