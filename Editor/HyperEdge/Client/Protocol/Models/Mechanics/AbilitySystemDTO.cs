using System;
using System.Collections.Generic;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol.Models.Mechanics
{
    [MessagePackObject(true)]
    public class AbilityGraphDTO
    {
        public Ulid Id { get; set; }
        public string Name { get; set; }
        public string Data { get; set; }
    }

    [MessagePackObject(true)]
    public class AbilityNodeDTO
    {
        public Ulid Id { get; set; }
        public string Name { get; set; }
        public string Base { get; set; }
        public string Category { get; set; }
        public string Code { get; set; }
    }

    [MessagePackObject(true)]
    public class AbilitySystemDTO
    {
        public Ulid Id { get; set; }
        public string Name { get; set; }
        public List<ContractFieldDTO> Stats;
        public List<string> Actors;
    }
}

