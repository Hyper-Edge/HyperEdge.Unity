using MessagePack;
using System;
using System.Collections.Generic;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class UpdateErc721TokenRequest
    {
        public Ulid Id { get; set; }
        public string Name { get; set; }
        public Erc721DataDTO DataJson { get; set; }
    }
}
