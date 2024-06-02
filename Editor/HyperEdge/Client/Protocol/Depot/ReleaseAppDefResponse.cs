using System;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class ReleaseAppDefResponse
    {
        public string JobId { get; set; }
    }
}
