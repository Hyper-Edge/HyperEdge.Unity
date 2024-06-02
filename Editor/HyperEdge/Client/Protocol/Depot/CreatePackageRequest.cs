using MessagePack;
using System;
using System.Collections.Generic;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class CreatePackageRequest
    {
        public Ulid StoreId { get; set; }
        public string Name { get; set; }
        public string Data { get; set; }
    }
}
