using System;


namespace HyperEdge.Sdk.Unity
{
    public class BuildAppVersionResponse
    {
        public string ServerFilesArchiveId { get; set; } = string.Empty;
        public string ServerImageId { get; set; } = string.Empty;
        public string  SyncBotImageId { get; set; } = string.Empty;
    }
}
