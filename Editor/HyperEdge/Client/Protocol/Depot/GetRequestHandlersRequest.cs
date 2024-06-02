using System;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class GetRequestHandlersRequest
    {
        public Ulid AppId { get; set; }
    }
}
