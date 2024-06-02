using System;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class AddCraftRulesResponse
    {
        public Ulid Id { get; set; }
        public string JobId { get; set; }
    }
}
