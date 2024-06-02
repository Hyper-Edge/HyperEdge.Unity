using System;
using System.Collections.Generic;


namespace HyperEdge.Sdk.Unity
{
    public class HeContainerInfo
    {
        public string Name;
        public string Status;
        public string AppId;
        public string EnvId;
        public string VersionId;
        public string ServerId;
    }

    public class GetAppContainersResponse
    {
        public List<HeContainerInfo> Containers { get; set; } = new();
    }
}
