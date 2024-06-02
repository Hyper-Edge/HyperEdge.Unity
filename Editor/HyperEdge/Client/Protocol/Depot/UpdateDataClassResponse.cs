using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class UpdateDataClassResponse
    {
        public DataClassContractDTO DataClassContract { get; set; }
    }
}
