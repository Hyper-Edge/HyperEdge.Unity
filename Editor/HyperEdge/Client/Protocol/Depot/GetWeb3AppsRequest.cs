using System;
using System.Collections.Generic;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class GetWeb3AppsRequest
    {
        public Ulid OwnerId { get; set; }
    }
}
