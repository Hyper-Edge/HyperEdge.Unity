using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class CreateWeb3AppResponse
    {
        public Web3AppDTO App { get; set; }
    }
}
