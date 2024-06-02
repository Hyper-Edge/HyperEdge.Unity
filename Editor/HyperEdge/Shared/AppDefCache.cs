using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MessagePipe;
using Newtonsoft.Json;
using UnityEngine;
using UnityEditor;

using HyperEdge.Shared.Protocol.Models.Export;


namespace HyperEdge.Sdk.Unity
{
    public class OnAppDefLoadedMsg
    {
        public string AppName { get; set; }
        public string VersionName { get; set; }
    }

    public class AppDefCache : IDisposable
    {
		private string GetAppVersionPath(string appName, string versionName)
		{
			return $"Assets/HyperEdge/{appName}/AppVersions/{versionName}.json";
		}

		private Dictionary<string, AppDef> _appDefs = new();
		
		private static AppDefCache _instance = new AppDefCache();
		public static AppDefCache Instance { get => _instance; }

        private IPublisher<OnAppDefLoadedMsg> _onAppDefLoadedPub;
        private ISubscriber<OnAppDefLoadedMsg> _onAppDefLoadedSub;
        public ISubscriber<OnAppDefLoadedMsg> OnAppDefLoaded
        {
            get => _onAppDefLoadedSub;
        }

        private readonly IDisposable _bag;        

        public AppDefCache()
        {
            (_onAppDefLoadedPub, _onAppDefLoadedSub) = GlobalMessagePipe.CreateEvent<OnAppDefLoadedMsg>();
            //
            var d = DisposableBag.CreateBuilder();
            MessageHub.Instance.OnPyCollectDone.Subscribe(r => OnPyCollectDone(r)).AddTo(d);
            _bag = d.Build();
        }

        public void Dispose()
        {
            _bag.Dispose();
        }

        private void OnPyCollectDone(OnPyCollectMessage pyMsg)
        {
            LoadAppDefFromStorage(pyMsg.AppName, "current");
        }

		public AppDef? GetCurrentAppDef(string appName)
		{
			return GetAppDef(appName, "current");
		}

        public AppDef LoadAppDefFromStorage(string appName, string versionName)
        {
            var appDefKey = $"{appName}@{versionName}";
            var appDefPath = GetAppVersionPath(appName, versionName);
            var appDefJson = File.ReadAllText(appDefPath);
            var appDefData = JsonConvert.DeserializeObject<AppDefDTO>(appDefJson);
            var appDef = new AppDef(appDefData);
            _appDefs[appDefKey] = appDef;
            _onAppDefLoadedPub.Publish(new OnAppDefLoadedMsg {
                AppName = appName,
                VersionName = versionName
            });
            return appDef;
        }

		public AppDef? GetAppDef(string appName, string versionName)
		{
            var appDefKey = $"{appName}@{versionName}";
			if (!_appDefs.TryGetValue(appDefKey, out var appDef))
			{
                return LoadAppDefFromStorage(appName, versionName);
			}
			return appDef;
		}
    }
}

