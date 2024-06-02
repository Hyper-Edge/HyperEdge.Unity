using System;
using System.Collections.Generic;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol.Models.Mechanics
{

    [MessagePackObject(true)]
    public class MultiPlayerSystemDTO
    {
        public Ulid Id { get; set; }
        public string Name { get; set; }
        public List<string> InputKeys { get; set; }
        public List<string> NetEntities { get; set; }
    }

    [MessagePackObject(true)]
    public class NetEntityFieldDTO
    {
        public string Name { get; set; }
        public string Typename { get; set; }
        public string DefaultValue { get; set; }
        public string InitFrom { get; set; }
    }

    [MessagePackObject(true)]
    public class NetEntityDTO
    {
        public Ulid Id { get; set; }
        public string Name { get; set; }
        public string Model { get; set; }
        public string Data { get; set; }
        public List<NetEntityFieldDTO> SyncFields { get; set; }
        public List<NetEntityFieldDTO> Fields { get; set; }
    }
}
