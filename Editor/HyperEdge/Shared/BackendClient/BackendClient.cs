using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

using HyperEdge.Shared.Services;
using HyperEdge.Shared.Protocol;
using HyperEdge.Shared.Protocol.Apps;
using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Sdk.Unity
{
    public class HyperEdgeBackendClient
    {
        private readonly string _host;
        public string Host { get => this._host; }
        
        public HyperEdgeBackendClient()
        {
            this._host = "https://" + HyperEdgeConstants.BackendUrl;
        }

        public async UniTask<GetAccountInfoResponse> GetAccountInfo()
        {
            var url = $"{this.Host}/api/bc/account";
            var resp = await GetJson<GetAccountInfoResponse>(url);
            //MessageHub.Instance.OnJobInfoResp(resp);
            return resp;
        }

        public async UniTask<AppEnvDTO> CreateAppEnvironment(string appId, string appEnvName)
        {
            var req = new CreateAppEnvRequest
            {
                AppId = Ulid.Parse(appId),
                Name = appEnvName
            };
            var jsonData = JsonConvert.SerializeObject(req);
            //var url = $"{this.Host}/api/IAppsService/CreateAppEnv";
            var url = $"{this.Host}/api/cd/processPlugin";
            var resp = await PostJson<CreateAppEnvResponse>(url, jsonData);
            return resp.AppEnv;
        }

        public async UniTask<List<Web3AppDTO>> GetApps()
        {
            var req = new GetWeb3AppsRequest
            {
                OwnerId = Ulid.Empty // TODO remove this?
            };
            var jsonData = JsonConvert.SerializeObject(req);
            var url = $"{this.Host}/api/IDepotService/GetWeb3Apps";
            var resp = await PostJson<GetWeb3AppsResponse>(url, jsonData);
            MessageHub.Instance.OnAppsInfoResp(resp);
            return resp.Apps;
        }

        public async UniTask<GetJobResponse> GetJobById(string jobId)
        {
            var url = $"{this.Host}/api/bc/tasks/{jobId}";
            var resp = await GetJson<GetJobResponse>(url);
            MessageHub.Instance.OnJobInfoResp(resp);
            return resp;
        }

        public async UniTask<GetAppAIProposalsResponse> GetAppAIProposals(string appId)
        {
            var url = $"{this.Host}/api/bc/apps/{appId}/ai/proposals";
            var resp = await GetJson<GetAppAIProposalsResponse>(url);
            MessageHub.Instance.OnAppAIProposalsResp(resp);
            return resp;
        }

        public async UniTask<GetAppAIProposalsResponse> GetAIThread(string thId)
        {
            var url = $"{this.Host}/api/bc/ai/threads/{thId}";
            var resp = await GetJson<GetAppAIProposalsResponse>(url);
            MessageHub.Instance.OnAppAIProposalsResp(resp);
            return resp;
        }

        public async UniTask<JobEnqueueResponse> GDDecompose(GDDecomposeRequest req)
        {
            var url = $"{this.Host}/api/bc/llm/gd_decompose";
            var jsonData = JsonConvert.SerializeObject(req);
            var resp = await PostJson<JobEnqueueResponse>(url, jsonData);
            return resp; 
        }

        public async UniTask<AppVersionInfoDTO> GetAppVersionInfo(string versionId)
        {
            var url = $"{this.Host}/api/bc/apps/versions/{versionId}";
            var resp = await GetJson<GetAppVersionResponse>(url);
            return resp.Version;
        }

        public async UniTask<List<AppVersionInfoDTO>> GetAppVersions(string appId)
        {
            var url = $"{this.Host}/api/bc/apps/{appId}/versions";
            var resp = await GetJson<GetAppVersionsResponse>(url);
            MessageHub.Instance.OnAppVersionsInfoResp(resp);
            return resp.Versions;
        }

        public async UniTask<List<AppEnvInfoDTO>> GetAppEnvironments(string appId)
        {
            var url = $"{this.Host}/api/bc/apps/{appId}/envs";
            var resp = await GetJson<GetAppEnvsResponse>(url);
            MessageHub.Instance.OnAppEnvsInfoResp(resp);
            return resp.Envs;
        }

        public async UniTask<List<HeImageInfo>> GetAppImages(string appId)
        {
            var url = $"{this.Host}/api/bc/apps/{appId}/images";
            var resp = await GetJson<GetAppImagesResponse>(url);
            MessageHub.Instance.OnAppImagesInfoResp(resp);
            return resp.Images;
        }

        public async UniTask<List<HeContainerInfo>> GetAppContainers(string appId)
        {
            var url = $"{this.Host}/api/bc/apps/{appId}/servers";
            var resp = await GetJson<GetAppContainersResponse>(url);
            MessageHub.Instance.OnContainersInfoResp(resp);
            return resp.Containers;
        }

        public async UniTask<GetAppUsersResponse> GetAppUsers(string appId)
        {
            var url = $"{this.Host}/api/bc/apps/{appId}/users";
            var resp = await GetJson<GetAppUsersResponse>(url);
            return resp;
        }

        public async UniTask<List<HeFileInfo>> GetAppFiles(string appId)
        {
            var url = $"{this.Host}/api/bc/files/{appId}";
            var resp = await GetJson<GetAppFilesResponse>(url);
            return resp.files;
        }

        public async UniTask<string> GetFileLink(string fileId)
        {
            var url = $"{this.Host}/api/bc/file/{fileId}";
            var resp = await GetJson<GetFileLinkResponse>(url);
            return resp.url;
        }

        public async Task DownloadFile(string url, string savePath)
        {
            using (var www = UnityWebRequest.Get(url))
            {
                www.downloadHandler = new DownloadHandlerFile(savePath);
                www.SetRequestHeader("X-API-Key", AppBuilderSettings.XApiKey);
                
                var webOp = www.SendWebRequest();
                while (!webOp.isDone)
                {
                    await Task.Yield();
                }

                if (www.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("File downloaded successfully: " + savePath);
                }
                else
                {
                    Debug.LogError("Error downloading file: " + www.error);
                }
            }
        }

        public async Task<string> DownloadFileById(string fileId, string savePath)
        {
            var fileUrl = await GetFileLink(fileId);
            await DownloadFile(fileUrl, savePath);
            return savePath;
        }

        public async UniTask<T> GetJson<T>(string url)
        {
            using var req = UnityWebRequest.Get(url);
            req.SetRequestHeader("X-API-Key", AppBuilderSettings.XApiKey);

            var webOp = req.SendWebRequest();
            while (!webOp.isDone)
            {
                await Task.Yield();
            }
            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed: {req.error}");
            }
            Debug.Log(req.downloadHandler.text);
            return JsonConvert.DeserializeObject<T>(req.downloadHandler.text);
        }

        public async UniTask<T> PostJson<T>(string url, string jsonData)
        {
            using var req = UnityWebRequest.Post(url, jsonData, "application/json");
            req.SetRequestHeader("X-API-Key", AppBuilderSettings.XApiKey);
            
            var webOp = req.SendWebRequest();
            while (!webOp.isDone)
            {
                await Task.Yield();
            }
            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed: {req.error}");
            }
            return JsonConvert.DeserializeObject<T>(req.downloadHandler.text);
        }
    }
}
