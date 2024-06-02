using System;
using System.Collections.Generic;
using MessagePack;


namespace HyperEdge.Shared.Protocol.Models.Export
{
    [MessagePackObject(true)]
    public class EventHandlerDTO
    {
        public Ulid Id { get; set; }
        public string Name { get; set; }
        public Ulid EventClassId { get; set; }
        public string EventClassName { get; set; }
        public string Code { get; set; }
    }
}
