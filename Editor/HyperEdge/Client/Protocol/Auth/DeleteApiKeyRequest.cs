using System;
using MessagePack;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class DeleteApiKeyRequest
    {
        public Ulid Id { get; set; }
    }
}
