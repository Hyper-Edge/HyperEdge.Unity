using System;
using MessagePack;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class GetAccountRequest
    {
        public Ulid Id { get; set; }
    }
}
