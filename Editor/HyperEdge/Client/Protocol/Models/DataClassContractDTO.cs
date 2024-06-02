using System;
using System.Collections.Generic;
using MessagePack;


namespace HyperEdge.Shared.Protocol.Models
{
    [MessagePackObject(true)]
    public class DataClassContractDTO
    {
        public Ulid Id { get; set; }
        public Ulid OwnerId { get; set; }
        public Ulid AppId { get; set; }
        public string AddressHex { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public string Data { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    [MessagePackObject(true)]
    public class DataClassContractDataDTO
    {
        public List<ContractFieldDTO> Fields { get; set; }
    }
}
