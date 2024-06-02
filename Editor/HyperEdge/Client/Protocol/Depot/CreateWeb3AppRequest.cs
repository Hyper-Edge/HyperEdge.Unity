using System;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class CreateWeb3AppRequest
    {
        public string Name { get; set; }
        public string Data { get; set; }
    }
}
