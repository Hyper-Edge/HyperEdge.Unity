using System;
using MessagePack;


namespace HyperEdge.Sdk.Shared.Protocol.Models
{
    [MessagePackObject(true)]
    public class PlayerAccountDTO
    {
        public Ulid Id { get; set; }

        public string DeviceUId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
        
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string AddressHex { get; set; } = string.Empty;

        public string EKSHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
