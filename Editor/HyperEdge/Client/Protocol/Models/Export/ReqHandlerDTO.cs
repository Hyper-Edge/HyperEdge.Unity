using System;
using System.Collections.Generic;
using MessagePack;


namespace HyperEdge.Shared.Protocol.Models.Export
{
    [MessagePackObject(true)]
    public class ReqHandlerDTO
    {
        public Ulid Id { get; set; }
        public string Name { get; set; }
        public Ulid RequestClassId { get; set; }
        public Ulid ResponseClassId { get; set; }
        public string RequestClassName { get; set; }
        public string ResponseClassName { get; set; }
        public string Code { get; set; }
    }
}
