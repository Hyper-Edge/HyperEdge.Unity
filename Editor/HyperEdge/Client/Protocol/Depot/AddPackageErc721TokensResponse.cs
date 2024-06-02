using System;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class AddPackageErc721TokensResponse
    {
        public PackageErc721TokensDTO PackageTokens { get; set; }
    }
}
