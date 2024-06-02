using MessagePack;


namespace HyperEdge.Sdk.Shared.Protocol
{
    [MessagePackObject(true)]
    public class ValidateGoogleReceiptRequest
    {
        public string ProductId { get; set; }
	    public string PurchaseToken { get; set; }
    }
}
