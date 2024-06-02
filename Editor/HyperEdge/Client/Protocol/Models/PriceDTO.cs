using System;
using MessagePack;


namespace HyperEdge.Shared.Protocol.Models
{
    [MessagePackObject(true)]
    public class PriceDTO
    {
        public Ulid Id { get; set; }
        public Ulid AppId { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
