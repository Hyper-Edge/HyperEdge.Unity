using System;
using System.Collections.Generic;
using MessagePack;

using HyperEdge.Sdk.Shared.Protocol.Models;


namespace HyperEdge.Sdk.Shared.Protocol
{
    [MessagePackObject(true)]
    public class ValidateReceiptsResponse
    {
        public bool Success { get; set; }
        public List<HyperEdgeReceiptInfo> Receipts { get; set; } = new();
    }
}
