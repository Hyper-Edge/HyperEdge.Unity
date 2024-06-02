using System;
using MessagePack;


namespace HyperEdge.Shared.Protocol.Models
{
    [MessagePackObject(true)]
    public class AppEnvDTO
    {
        public Ulid Id { get; set; }
        public Ulid AppId { get; set; }
        public string Name { get; set; }
    }
}