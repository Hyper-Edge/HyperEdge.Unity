using System;
using MessagePack;


namespace HyperEdge.Shared.Protocol.Apps
{
    [MessagePackObject(true)]
    public class RunAppRequest
    {
        public Ulid AppId { get; set; }
        public Ulid VersionId { get; set; }
        public Ulid EnvId { get; set; }
    }
}
