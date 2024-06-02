using System;
using System.Collections.Generic;
using MessagePack;


namespace HyperEdge.Shared.Protocol.Models.Export
{
    [MessagePackObject(true)]
    public class DataClassInstanceDTO
    {
        public Ulid Id { get; set; }
        public string Name { get; set; }
        public List<DataClassInstanceFieldDTO> Fields { get; set; } = new();
    }

    [MessagePackObject(true)]
    public class DataClassInstanceFieldDTO
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
