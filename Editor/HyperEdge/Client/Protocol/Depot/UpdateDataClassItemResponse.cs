using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class UpdateDataClassItemResponse
    {
        public DataClassItemDTO DataClassItem { get; set; }
    }
}
