using System;
using System.Runtime.Serialization;
using MessagePack;


namespace HyperEdge.Shared.Protocol.Models
{
    [MessagePackObject(true)]
    public class Web3AppDTO
    {
        public Ulid Id { get; set; }
        public Ulid OwnerId { get; set; }
        public string Name { get; set; }
        public string Data { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    [MessagePackObject(true)]
    public class Web3AppDataDTO
    {
        public string DiamondCutFacetAddress = string.Empty;
        public string DiamondLoupeFacetAddress = string.Empty;
        public string OwnershipFacetAddress = string.Empty;
        public string DiamondAddress = string.Empty;
        public string InventoryFacetAddress = string.Empty;
        public string ShopFacetAddress = string.Empty;
        public string MarketFacetAddress = string.Empty;
    }
}
