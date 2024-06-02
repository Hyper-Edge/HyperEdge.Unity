using System;
using System.Collections.Generic;
using MessagePack;


namespace HyperEdge.Shared.Protocol.Models.Mechanics
{
    [MessagePackObject(true)]
    public class Erc721Cost
    {
        public string EntityName { get; set; }
        public string ItemId { get; set; }
        public ulong Amount { get; set; }
        public List<string> Conditions { get; set; }
    }

    [MessagePackObject(true)]
    public class Erc1155Cost
    {
        public string ItemId { get; set; }
        public ulong Amount { get; set; }
    }

    [MessagePackObject(true)]
    public class CostDTO
    {
        public List<Erc721Cost> Erc721Costs { get; set; } = new();
        public List<Erc1155Cost> Erc1155Costs { get; set; } = new();
    }
}
