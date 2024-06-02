using System;
using System.Collections.Generic;
using MessagePack;

using HyperEdge.Shared.Protocol.Models.Export;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class ReleaseAppDefRequest
    {
        public Ulid AppId { get; set; }
        public string VersionName { get; set; }
        public AppDefDTO AppDef { get; set; }
    }
}
