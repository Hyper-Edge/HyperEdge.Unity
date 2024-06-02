using System;
using MessagePack;


namespace HyperEdge.Sdk.Shared.Protocol.Models
{
    [MessagePackObject(true)]
    public class HyperEdgeReceiptInfo
    {
        public Ulid ReceiptId { get; set; }
        public string ProductId { get; set; }
        public string TxId { get; set; }
        public int ProviderType { get; set; }
        public string ProviderResponse { get; set; }
    }
}