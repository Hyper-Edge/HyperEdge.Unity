using System;
using System.Collections.Generic;
using MessagePack;


namespace HyperEdge.Shared.Protocol.Models.Export
{
    [MessagePackObject(true)]
    public class UserGroupClassDTO
    {
        public Ulid Id { get; set; }
        public string Name { get; set; }
        public List<DataClassFieldDTO> Fields { get; set; }
        public List<string> StorageClasses { get; set; }
    }
}
