using System;
using MessagePack;


namespace HyperEdge.Shared.Protocol.Apps
{
    [MessagePackObject(true)]
    public class BuildAppResponse
    {
        public string JobId { get; set; }
    }
}
