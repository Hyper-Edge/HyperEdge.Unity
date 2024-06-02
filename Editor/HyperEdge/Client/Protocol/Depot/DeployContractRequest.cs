using System;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class DeployContractRequest
    {
        public Ulid Id { get; set; }
        public string ContractType { get; set; }
    }
}
