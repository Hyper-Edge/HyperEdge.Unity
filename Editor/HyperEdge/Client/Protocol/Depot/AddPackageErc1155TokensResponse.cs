using System;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class AddPackageErc1155TokensResponse
    {
        public PackageErc1155TokensDTO PackageTokens { get; set; }
    }
}
