using MessagePack;
using System;
using System.Collections.Generic;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class CreateErc721TokenRequest
    {
        public string Name { get; set; }
        public Ulid AppId { get; set; }
        public Erc721DataDTO DataJson { get; set; }
    }

    [MessagePackObject(true)]
    public class Erc721DataDTO
    {
        public List<ContractFieldDTO> Fields { get; set; }
    }
}
