using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class CreateDataClassItemResponse
    {
        public DataClassItemDTO DataClassItem { get; set; }
    }
}
