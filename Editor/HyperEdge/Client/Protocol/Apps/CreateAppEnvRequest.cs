using System;
using MessagePack;


namespace HyperEdge.Shared.Protocol.Apps
{
    [MessagePackObject(true)]
    public class CreateAppEnvRequest
    {
        public Ulid AppId { get; set; }
        public string Name { get; set; }
    }
}
