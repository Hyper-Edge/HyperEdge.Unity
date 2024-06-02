using System;
using System.Collections.Generic;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class GetDataClassItemsResponse
    {
        public List<DataClassItemDTO> Items { get; set; }
        public long NumPages { get; set; }
        public long Total { get; set; }
    }
}