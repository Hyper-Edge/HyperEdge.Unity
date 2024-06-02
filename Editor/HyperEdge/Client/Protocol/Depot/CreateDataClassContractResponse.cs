using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class CreateDataClassContractResponse
    {
        public DataClassContractDTO DataClassContract { get; set; }
    }
}
