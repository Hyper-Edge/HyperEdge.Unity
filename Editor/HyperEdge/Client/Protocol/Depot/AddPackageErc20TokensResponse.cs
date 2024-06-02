using System;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class AddPackageErc20TokensResponse
    {
        public PackageErc20TokensDTO PackageTokens { get; set; }
    }
}