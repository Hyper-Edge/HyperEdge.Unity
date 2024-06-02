using System;
using System.Collections.Generic;


namespace HyperEdge.Sdk.Unity
{
    public class HeImageInfo
    {
        public string Id;
        public string AppId;
        public string VersionId;
        public string HeComponent;
    }

    public class GetAppImagesResponse
    {
        public List<HeImageInfo> Images { get; set; } = new();
    }
}
