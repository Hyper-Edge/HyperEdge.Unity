using System;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class AddRequestHandlerRequest
    {

        public Ulid AppId { get; set; }
        
        public string Name { get; set; }
        public Ulid RequestClassId { get; set; }
        public Ulid ResponseClassId { get; set; }
    }
}
