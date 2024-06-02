using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class GetAccountResponse
    {
        public AccountDTO Account { get; set; }
    }
}
