using System;
using MessagePack;


namespace HyperEdge.Shared.Protocol.Models
{
    [MessagePackObject(true)]
    public class PackageErc1155TokensDTO
    {
        public Ulid Id { get; set; }
        public Ulid PackageId { get; set; }
        public Ulid TokenId { get; set; }
        public Ulid ItemId { get; set; }
        public ulong Amount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
