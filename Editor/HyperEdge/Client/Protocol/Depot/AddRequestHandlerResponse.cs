using System;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class AddRequestHandlerResponse
    {
        public RequestHandlerDTO Handler { get; set; }
    }
}
