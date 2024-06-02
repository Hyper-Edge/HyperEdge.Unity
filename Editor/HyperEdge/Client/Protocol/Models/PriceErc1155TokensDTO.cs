using System;
using MessagePack;


namespace HyperEdge.Shared.Protocol.Models
{
    [MessagePackObject(true)]
    public class PriceErc1155TokensDTO
    {
        public Ulid Id { get; set; }
        public Ulid PriceId { get; set; }
        public Ulid TokenId { get; set; }
        public Ulid ItemId { get; set; }
        public ulong Amount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}