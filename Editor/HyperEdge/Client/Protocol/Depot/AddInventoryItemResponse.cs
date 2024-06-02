using System;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class AddInventoryItemResponse
    {
        public Erc1155ItemDTO Item { get; set; }
    }
}
