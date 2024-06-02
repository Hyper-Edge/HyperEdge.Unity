using System;
using MessagePack;


namespace HyperEdge.Shared.Protocol.Models
{
    [MessagePackObject(true)]
    public class Erc1155ItemDTO
    {
        public Ulid Id { get; set; }
        public Ulid AppId { get; set; }
        public Ulid TokenId { get; set; }
        public Ulid DataClassItem { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
