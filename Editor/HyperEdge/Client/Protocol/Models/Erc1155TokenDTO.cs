using System;
using MessagePack;


namespace HyperEdge.Shared.Protocol.Models
{
    [MessagePackObject(true)]
    public class Erc1155TokenDTO
    {
        public Ulid Id { get; set; }
        public Ulid OwnerId { get; set; }
        public Ulid AppId { get; set; }
        public string AddressHex { get; set; }
        public string Data { get; set; }
        public string Abi { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

