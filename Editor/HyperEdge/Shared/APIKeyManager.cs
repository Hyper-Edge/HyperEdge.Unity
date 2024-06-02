using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEditor;


namespace HyperEdge.Sdk.Unity
{
    public class APIKeyManager
    {
        private static APIKeyManager _instance = new();
        public static APIKeyManager Instance { get => _instance; }
        public static bool LoggedIn { get => (APIKeyManager.Instance.UserId != string.Empty); }

        public string UserId { get; private set; } = string.Empty;

        public APIKeyManager()
        {
        }        

        public async UniTask<bool> CheckAPIKey(string apiKey)
        {
            AppBuilderSettings.XApiKey = apiKey;
            var heClient = new HyperEdgeBackendClient();
            try
            {
                var accInfo = await heClient.GetAccountInfo();
                if (!accInfo.Success)
                {
                    this.UserId = string.Empty;
                    AppBuilderSettings.XApiKey = string.Empty;
                    return false;
                }
                this.UserId = accInfo.UserId;
                return true;
            }
            catch
            {
                this.UserId = string.Empty;
                AppBuilderSettings.XApiKey = string.Empty;
                return false;
            }
        }
    }
}

