using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class CreateErc721TokenResponse
    {
        public Erc721TokenDTO Token { get; set; }
    }
}
