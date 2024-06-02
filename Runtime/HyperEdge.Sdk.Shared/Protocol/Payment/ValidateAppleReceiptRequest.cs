using MessagePack;


namespace HyperEdge.Sdk.Shared.Protocol
{
    [MessagePackObject(true)]
    public class ValidateAppleReceiptRequest
    {
        public string ReceiptData { get; set; }
    }
}
