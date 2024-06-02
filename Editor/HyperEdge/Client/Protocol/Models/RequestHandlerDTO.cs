using System;
using MessagePack;


namespace HyperEdge.Shared.Protocol.Models
{
    [MessagePackObject(true)]
    public class RequestHandlerDTO
    {
        public Ulid Id { get; set; }
        public Ulid UserId { get; set; }
        public Ulid AppId { get; set; }
        
        public string Name { get; set; }
        public Ulid RequestClassId { get; set; }
        public Ulid ResponseClassId { get; set; }

        //public string Data { get; set; }
    }
}
