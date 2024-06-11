using System;
using System.Collections.Generic;
using MessagePack;


namespace HyperEdge.Shared.Protocol.Models.Export
{
    [MessagePackObject(true)]
    public class DataClassDTO
    {
        public Ulid Id { get; set; }
        public string Name { get; set; }
        public string Base { get; set; }
        public List<DataClassFieldDTO> Fields { get; set; } = new();

        public DataClassDTO Clone()
        {
            var bs = MessagePackSerializer.Serialize(this);
            return MessagePackSerializer.Deserialize<DataClassDTO>(bs);
        }
    }

    [MessagePackObject(true)]
    public class DataClassFieldDTO
    {
        public string Name { get; set; }
        public string Typename { get; set; }
        public string DefaultValue { get; set; }
    }

    [MessagePackObject(true)]
    public class DataClassFieldsDTO
    {
        public List<ContractFieldDTO> Fields { get; set; } = new();
    }
}
