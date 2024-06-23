using System;
using System.Collections.Generic;
using MessagePack;

using HyperEdge.Shared.Protocol.Models.Export;


namespace HyperEdge.Shared.Protocol.Models
{
    [MessagePackObject(true)]
    public class DataClassItemDTO
    {
        public Ulid Id { get; set; }
        public Ulid AppId { get; set; }
        public Ulid DataClassContract { get; set; }
        public string Name { get; set; }
        public string Data { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    [MessagePackObject(true)]
    public class DataClassItemDataDTO
    {
        public List<DataClassInstanceFieldDTO> Fields { get; set; }
    }
}
