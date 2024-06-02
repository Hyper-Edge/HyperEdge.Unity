using System;
using System.Collections.Generic;
using MessagePack;


namespace HyperEdge.Shared.Protocol.Models.Export
{
    [MessagePackObject(true)]
    public class JobHandlerDTO
    {
        public Ulid Id { get; set; }
        public string Name { get; set; }
        public Ulid JobDataClassId { get; set; }
        public string JobDataClassName { get; set; }
        public string Code { get; set; }
    }
}
