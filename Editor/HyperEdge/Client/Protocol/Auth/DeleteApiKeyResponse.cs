using System;
using MessagePack;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class DeleteApiKeyResponse
    {
        public bool Success { get; set; }
    }
}
