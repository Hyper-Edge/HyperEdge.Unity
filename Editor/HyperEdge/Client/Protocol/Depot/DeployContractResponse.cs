using System;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class DeployContractResponse
    {
        public bool Success { get; set; }
        public string TaskId { get; set; }
    }
}
