using System;
using MessagePack;


namespace HyperEdge.Sdk.Shared.Protocol
{
    [MessagePackObject(true)]
    public class CheckHealthResponse
    {
        public Ulid AppId { get; set; }
        public bool Healthy { get; set; }
    }
}
