using System;
using MessagePack;


namespace HyperEdge.Shared.Protocol.Apps
{
    [MessagePackObject(true)]
    public class BuildAppRequest
    {
        public Ulid AppId { get; set; }
        public string VersionName { get; set; }
    }
}
