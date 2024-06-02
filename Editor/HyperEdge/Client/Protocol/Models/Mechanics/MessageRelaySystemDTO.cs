using System;
using System.Collections.Generic;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;
using HyperEdge.Shared.Protocol.Models.Export;


namespace HyperEdge.Shared.Protocol.Models.Mechanics
{
    [MessagePackObject(true)]
    public class MessageRelayTypeDTO
    {
        public string Name { get; set; }
        public string RequestClassName { get; set; }
        public string ResponseClassName { get; set; }
        public bool Relay { get; set; }
        public string Code { get; set; }
    }

    [MessagePackObject(true)]
    public class MessageRelaySystemDTO
    {
        public Ulid Id { get; set; }
        public string Name { get; set; }
        public bool Standalone { get; set; }
        //
        public List<MessageRelayTypeDTO> MessageTypes { get; set; }
    } 
}