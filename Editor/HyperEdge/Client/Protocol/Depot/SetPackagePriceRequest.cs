using System;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class SetPackagePriceRequest
    {
        public Ulid PackageId { get; set; }
        public Ulid PriceId { get; set; }
    }
}
