using System;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class AddPackageErc1155TokensRequest
    {
        public Ulid PackageId { get; set; }
        public Ulid TokenId { get; set; }
        public Ulid ItemId { get; set; }
        public ulong Amount { get; set; }
    }
}