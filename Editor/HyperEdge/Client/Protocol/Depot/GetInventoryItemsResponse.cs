using System;
using System.Collections.Generic;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class GetInventoryItemsResponse
    {
        public List<Erc1155ItemDTO> Items { get; set; }
    }
}
