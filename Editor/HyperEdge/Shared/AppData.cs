using System;
using System.Collections.Generic;
using UnityEngine;


namespace HyperEdge.Sdk.Unity
{
    [Serializable]
    public class AppData : ScriptableObject
    {
        public string Id = string.Empty;
        public string Name = string.Empty;
        public string Description = string.Empty;
        public List<AppVersionInfo> Versions = new List<AppVersionInfo>();
        public List<AppEnvironmentInfo> AppEnvironments = new List<AppEnvironmentInfo>();
        public List<HeFileInfo> Files = new List<HeFileInfo>();

        public static AppData? Load()
        {
            var l = AssetUtils.FindAssetsByType<AppData>();
            return l.Count == 0 ? null : l[0];
        }  
        
        public static AppData? LoadByAppId(string appId)
        {
            var allAppData = AssetUtils.FindAssetsByType<AppData>();
            foreach (var appData in allAppData)
            {
                if (appData.Id == appId)
                {
                    return appData;
                }
            }
            return null;
        }
    }

    [Serializable]
    public class AppVersionInfo
    {
        public string Id = string.Empty;
        public string Name = string.Empty;
        public string LlmProposalId = string.Empty;
        public AppVersionFilesInfo Files = new();
        public AppVersionImagesInfo Images = new();
    }

    [Serializable]
    public class AppVersionFilesInfo
    {
        public string SdkSharedFileId = string.Empty;
        public string SdkServerFileId = string.Empty;
        public string SdkNodeFlowsFileId = string.Empty;
        public string GameDataFileId = string.Empty;
        public string UnityClientFileId = string.Empty;
        public string TurnBattlerFileId = string.Empty;
        public string NetPlayFileId = string.Empty;

        public bool Empty
        {
            get
            {
                if (string.IsNullOrEmpty(SdkSharedFileId)) return true;
                if (string.IsNullOrEmpty(SdkServerFileId)) return true;
                if (string.IsNullOrEmpty(SdkNodeFlowsFileId)) return true;
                if (string.IsNullOrEmpty(UnityClientFileId)) return true;
                //if (string.IsNullOrEmpty(TurnBattlerFileId)) return true;
                //if (string.IsNullOrEmpty(NetPlayFileId)) return true;
                return false;
            }
        }
    }

    [Serializable]
    public class AppVersionImagesInfo
    {
        public string ServerImageId = string.Empty;
        public string SyncBotImageId = string.Empty;
    }

    [Serializable]
    public class AppEnvironmentInfo
    {
        public string Id = string.Empty;
        public string Name = string.Empty;
    }
}

