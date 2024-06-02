using System;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol.Apps
{
    [MessagePackObject(true)]
    public class CreateAppEnvResponse
    {
        public AppEnvDTO AppEnv { get; set; }
    }
}
