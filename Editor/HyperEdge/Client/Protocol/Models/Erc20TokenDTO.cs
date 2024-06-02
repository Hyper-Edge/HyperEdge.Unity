using System;
using MessagePack;


namespace HyperEdge.Shared.Protocol.Models
{
    [MessagePackObject(true)]
    public class Erc20TokenDTO
    {
        public Ulid Id { get; set; }
        public Ulid OwnerId { get; set; }
        public Ulid AppId { get; set; }
        public string AddressHex { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        public string Data { get; set; }
        public string Abi { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

