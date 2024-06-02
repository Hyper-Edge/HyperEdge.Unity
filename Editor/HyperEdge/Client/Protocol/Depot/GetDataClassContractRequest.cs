using System;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class GetDataClassContractRequest
    {
        public Ulid Id { get; set; }
    }
}
