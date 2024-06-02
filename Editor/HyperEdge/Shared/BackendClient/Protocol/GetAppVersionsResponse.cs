using System;
using System.Collections.Generic;


namespace HyperEdge.Sdk.Unity
{
    public class GetAppVersionsResponse
    {
        public List<AppVersionInfoDTO> Versions { get; set; }
    }

    public class GetAppVersionResponse
    {
        public AppVersionInfoDTO Version { get; set; }
    }

    public class AppVersionInfoData
    {
        public string AppDefFileId { get; set; }
        public AppVersionInfoFilesData Files { get; set; }
        public AppVersionInfoImagesData Images { get; set; }
    }

    public class AppVersionInfoFilesData
    {
        public string SdkSharedFileId { get; set; } = string.Empty;
        public string SdkServerFileId { get; set; } = string.Empty;
        public string SdkNodeFlowsFileId { get; set; } = string.Empty;
        public string GameDataFileId { get; set; } = string.Empty;
        public string UnityClientFileId { get; set; } = string.Empty;
        public string TurnBattlerFileId { get; set; } = string.Empty;
        public string NetPlayFileId { get; set; } = string.Empty;
    }

    public class AppVersionInfoImagesData
    {
        public string ServerImageId { get; set; } = string.Empty;
        public string SyncBotImageId { get; set; } = string.Empty;
    }

    public class AppVersionInfoDTO
    {
        public string Id { get; set; }
        public string AppId { get; set; }
        public string Name { get; set; }
        public AppVersionInfoData Data { get; set; }
    }
}
