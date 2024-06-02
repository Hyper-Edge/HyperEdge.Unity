using System;
using System.Collections.Generic;
using MessagePack;


namespace HyperEdge.Shared.Protocol.Models.Export
{
    [MessagePackObject(true)]
    public class InventoryDefDTO
    {
        public string Name { get; set; }
        public List<InventoryDefItemDTO> Items { get; set; }
    }

    [MessagePackObject(true)]
    public class InventoryDefItemDTO
    {
        public string Id { get; set; }
        public string Typename { get; set; }
    }
}