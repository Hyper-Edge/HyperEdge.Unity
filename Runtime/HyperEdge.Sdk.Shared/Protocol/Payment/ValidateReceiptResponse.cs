using System;
using MessagePack;

using HyperEdge.Sdk.Shared.Protocol.Models;


namespace HyperEdge.Sdk.Shared.Protocol
{
    [MessagePackObject(true)]
    public class ValidateReceiptResponse
    {
        public bool Success { get; set; }
        public HyperEdgeReceiptInfo ReceiptInfo { get; set; }
    }
}
