using System;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class ExportAppDefResponse
    {
        public string JobId { get; set; }
    }
}
