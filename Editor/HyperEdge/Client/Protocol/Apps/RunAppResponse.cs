using System;
using MessagePack;


namespace HyperEdge.Shared.Protocol.Apps
{
    [MessagePackObject(true)]
    public class RunAppResponse
    {
        public string JobId { get; set; }
    }
}
