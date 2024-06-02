using MessagePack;
using System;
using System.Collections.Generic;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class CreatePriceRequest
    {
        public Ulid AppId { get; set; }
        public string Name { get; set; }
    }
}
