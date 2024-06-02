using System;
using System.Collections.Generic;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class GetWeb3AppsResponse
    {
        public List<Web3AppDTO> Apps { get; set; }
    }
}
