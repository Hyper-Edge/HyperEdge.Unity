using System;
using System.Collections.Generic;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class GetStoresResponse
    {
        public List<StoreDTO> Stores { get; set; }
        public uint NumPages { get; set; }
    }
}
