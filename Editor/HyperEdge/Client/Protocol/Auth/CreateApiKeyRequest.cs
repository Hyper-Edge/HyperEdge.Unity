using System;
using MessagePack;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class CreateApiKeyRequest
    {
        public string Name { get; set; }
        public string Data { get; set; }
    }
}
