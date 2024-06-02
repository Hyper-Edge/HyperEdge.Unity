using System;


namespace HyperEdge.Sdk.Unity
{
    public class BuildAppVersionRequest
    {
        public Ulid AppId { get; set; }
        public string VersionName { get; set; }
    }
}
