using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using MagicOnion;
using MagicOnion.Client;
using Newtonsoft.Json;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

using HyperEdge.Sdk.Shared.Services;
using HyperEdge.Sdk.Shared.Protocol;


namespace HyperEdge.Sdk.Unity.APITester
{
    public class ServerClient
    {
        private readonly string _wgHost;
        public string WebGateHost { get => this._wgHost; }

        private IClientFilter[] _clientFilters;
        private readonly ServerInfo _serverInfo;
        private readonly GrpcChannelx _channel;

        private AccountData? _accData = null;

        public ServerClient(ServerInfo serverInfo)
        {
            _serverInfo = serverInfo;
            Debug.Log($"Url: {_serverInfo.Url}");
            _channel = GrpcChannelx.ForAddress(_serverInfo.Url);
            _clientFilters = new IClientFilter[] { new WithAppIdFilter(serverInfo) };
            //
            this._wgHost = "https://" + HyperEdgeConstants.BackendUrl + "/gs/api/";
        }

        public void SetAccount(AccountData accData)
        {
            _accData = accData;
        }

        public IHealthService GetHealthService()
        {
            return MagicOnionClient.Create<IHealthService>(_channel, _clientFilters);
        }

        public IAccountService GetAccountService()
        {
            return MagicOnionClient.Create<IAccountService>(_channel, _clientFilters);
        }

        public IAuthService GetAuthService()
        {
            return MagicOnionClient.Create<IAuthService>(_channel, _clientFilters);
        }

        public void CheckHealth()
        {
            CheckHealthAsync().Forget();
        }

        public async UniTaskVoid CheckHealthAsync()
        {
            var resp = await GetHealthService().CheckHealth();
            MessageHub.Instance.PublishServerHealthInfo(new OnServerHealthInfoMsg
            {
                AppId = resp.AppId.ToString(),
                VersionId = _serverInfo.VersionId,
                EnvId = _serverInfo.EnvId,
                Healthy = resp.Healthy
            });
        }

        public async UniTaskVoid CheckHealthWebAsync()
        {
            var resp = await PostJson<CheckHealthResponse>("IHealthService/CheckHealth", "{}");
            MessageHub.Instance.PublishServerHealthInfo(new OnServerHealthInfoMsg
            {
                AppId = resp.AppId.ToString(),
                VersionId = _serverInfo.VersionId,
                EnvId = _serverInfo.EnvId,
                Healthy = resp.Healthy
            });
        }

        public UniTask<string> GetCurrentUserAsync()
        {
            return CallWebGateway("IGameService/GetCurrentUser", "");
        }

        public UniTask<string> AddItems(string itemId, ulong amount)
        {
            return CallWebGateway($"ITestService/AddItems",
                $"{{\"ItemId\": \"{itemId}\", \"Amount\": {amount}}}");
        }

        public UniTask<string> AddEntity(string entityName, string itemId, ulong amount)
        {           
            return CallWebGateway($"ITestService/Add{entityName}",
                $"{{\"Type\": \"{itemId}\", \"Amount\": {amount}}}");
        }

        private async UniTask<string> CallWebGateway(string url, string jsonData)
        {
            using var req = UnityWebRequest.Post(WebGateHost + url, jsonData, "application/json");
            req.SetRequestHeader("ServerId", _serverInfo.ServerId);
            // Auth
            if (_accData is not null)
            {
                var tokenData = AuthTokenStorage.Current.GetToken(_accData.UserId);
                if (tokenData is not null && !tokenData.IsExpired)
                {
                    Debug.Log($"Auth: {tokenData.Token}");
                    req.SetRequestHeader("Authorization", "Bearer " + tokenData.Token);
                }
            }
            //
            var webOp = req.SendWebRequest();
            while (!webOp.isDone)
            {
                await UniTask.Yield();
            }
            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed: {req.error}");
            }
            return req.downloadHandler.text;
        }
 
        public async UniTask<T> PostJson<T>(string url, string jsonData)
        {
            var ret = await CallWebGateway(url, jsonData);
            return JsonConvert.DeserializeObject<T>(ret);
        }
    }
}
