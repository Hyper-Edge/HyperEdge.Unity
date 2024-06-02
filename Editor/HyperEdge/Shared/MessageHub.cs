using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEditor;

using HyperEdge.Shared.Protocol;
using HyperEdge.Shared.Protocol.Models;
using HyperEdge.Shared.Protocol.Models.Export;


namespace HyperEdge.Sdk.Unity
{
    public class MessageHub
    {
		private static MessageHub _instance = new MessageHub();
		public static MessageHub Instance { get => _instance; }

        private IPublisher<GetAppContainersResponse> _onContainersInfoPub;
        private ISubscriber<GetAppContainersResponse> _onContainersInfoSub;
        //
        private IPublisher<GetAppImagesResponse> _onImagesInfoPub;
        private ISubscriber<GetAppImagesResponse> _onImagesInfoSub;
        //
        private IPublisher<GetAppVersionsResponse> _onAppVersionsInfoPub;
        private ISubscriber<GetAppVersionsResponse> _onAppVersionsInfoSub;
        //
        private IPublisher<GetAppEnvsResponse> _onAppEnvsInfoPub;
        private ISubscriber<GetAppEnvsResponse> _onAppEnvsInfoSub;
        //
        private IPublisher<GetWeb3AppsResponse> _onAppsInfoPub;
        private ISubscriber<GetWeb3AppsResponse> _onAppsInfoSub;
        //
        private IPublisher<OnServerHealthInfoMsg> _onServerHealthInfoPub;
        private ISubscriber<OnServerHealthInfoMsg> _onServerHealthInfoSub;
        //
        private IPublisher<GetAppAIProposalsResponse> _onAppAIProposalsPub;
        private ISubscriber<GetAppAIProposalsResponse> _onAppAIProposalsSub;
        //
        private IPublisher<GetJobResponse> _onJobInfoPub;
        private ISubscriber<GetJobResponse> _onJobInfoSub;
        //
        private IPublisher<string> _onUniPipeMessagePub;
        private ISubscriber<string> _onUniPipeMessageSub;
        //
        private IPublisher<OnPyCollectMessage> _onPyCollectPub;
        private ISubscriber<OnPyCollectMessage> _onPyCollectSub;
        //
        private IPublisher<OnPyExportMessage> _onPyExportPub;
        private ISubscriber<OnPyExportMessage> _onPyExportSub;
        //
        private IPublisher<OnPyServerStartMessage> _onPyServerStartPub;
        private ISubscriber<OnPyServerStartMessage> _onPyServerStartSub;
        //
        private IPublisher<OnPyServerStopMessage> _onPyServerStopPub;
        private ISubscriber<OnPyServerStopMessage> _onPyServerStopSub;
        //
        private IPublisher<OnPyLlmGenDataMessage> _onPyLlmGenDataPub;
        private ISubscriber<OnPyLlmGenDataMessage> _onPyLlmGenDataSub;
        //
        private IPublisher<OnPyLlmProposeModelMessage> _onPyLlmProposeModelPub;
        private ISubscriber<OnPyLlmProposeModelMessage> _onPyLlmProposeModelSub;
        //
        private IPublisher<OnPyLlmProposalExportMessage> _onPyLlmProposalExportPub;
        private ISubscriber<OnPyLlmProposalExportMessage> _onPyLlmProposalExportSub;

        private Dictionary<string, Action<JObject>> _pyHandlers = new();

        public ISubscriber<GetAppImagesResponse> OnImagesInfo
        {
            get => _onImagesInfoSub;
        }

        public ISubscriber<GetAppContainersResponse> OnContainersInfo
        {
            get => _onContainersInfoSub;
        }

        public ISubscriber<GetAppVersionsResponse> OnAppVersionsInfo
        {
            get => _onAppVersionsInfoSub;
        }

        public ISubscriber<GetAppEnvsResponse> OnAppEnvsInfo
        {
            get => _onAppEnvsInfoSub;
        }

        public ISubscriber<GetWeb3AppsResponse> OnAppsInfo
        {
            get => _onAppsInfoSub;
        }

        public ISubscriber<GetAppAIProposalsResponse> OnAppAIProposals
        {
            get => _onAppAIProposalsSub;
        }

        public ISubscriber<GetJobResponse> OnJobInfo
        {
            get => _onJobInfoSub;
        }

        public ISubscriber<OnServerHealthInfoMsg> OnServerHealthInfo
        {
            get => _onServerHealthInfoSub;
        }

        public ISubscriber<OnPyCollectMessage> OnPyCollectDone
        {
            get => _onPyCollectSub;
        }

        public ISubscriber<OnPyExportMessage> OnPyExportDone
        {
            get => _onPyExportSub;
        }

        public ISubscriber<OnPyLlmGenDataMessage> OnPyLlmGenDataDone
        {
            get => _onPyLlmGenDataSub;
        }

        public ISubscriber<OnPyLlmProposeModelMessage> OnPyLlmProposeModelDone
        {
            get => _onPyLlmProposeModelSub;
        }

        public ISubscriber<OnPyLlmProposalExportMessage> OnPyLlmProposalExportDone
        {
            get => _onPyLlmProposalExportSub;
        }

        public ISubscriber<OnPyServerStartMessage> OnPyServerStartDone
        {
            get => _onPyServerStartSub;
        }

        public ISubscriber<OnPyServerStopMessage> OnPyServerStopDone
        {
            get => _onPyServerStopSub;
        }

        public MessageHub()
        {
            //
            (_onContainersInfoPub, _onContainersInfoSub) = GlobalMessagePipe.CreateEvent<GetAppContainersResponse>();
            (_onImagesInfoPub, _onImagesInfoSub) = GlobalMessagePipe.CreateEvent<GetAppImagesResponse>();
            (_onAppVersionsInfoPub, _onAppVersionsInfoSub) = GlobalMessagePipe.CreateEvent<GetAppVersionsResponse>();
            (_onAppEnvsInfoPub, _onAppEnvsInfoSub) = GlobalMessagePipe.CreateEvent<GetAppEnvsResponse>();
            (_onAppsInfoPub, _onAppsInfoSub) = GlobalMessagePipe.CreateEvent<GetWeb3AppsResponse>();
            (_onServerHealthInfoPub, _onServerHealthInfoSub) = GlobalMessagePipe.CreateEvent<OnServerHealthInfoMsg>();
            //
            (_onAppAIProposalsPub, _onAppAIProposalsSub) = GlobalMessagePipe.CreateEvent<GetAppAIProposalsResponse>();
            //
            (_onJobInfoPub, _onJobInfoSub) = GlobalMessagePipe.CreateEvent<GetJobResponse>();
            //
            (_onUniPipeMessagePub, _onUniPipeMessageSub) = GlobalMessagePipe.CreateEvent<string>();
            //
            (_onPyCollectPub, _onPyCollectSub) = GlobalMessagePipe.CreateEvent<OnPyCollectMessage>();
            (_onPyExportPub, _onPyExportSub) = GlobalMessagePipe.CreateEvent<OnPyExportMessage>();
            //
            (_onPyLlmGenDataPub, _onPyLlmGenDataSub) = GlobalMessagePipe.CreateEvent<OnPyLlmGenDataMessage>();
            (_onPyLlmProposeModelPub, _onPyLlmProposeModelSub) = GlobalMessagePipe.CreateEvent<OnPyLlmProposeModelMessage>();
            (_onPyLlmProposalExportPub, _onPyLlmProposalExportSub) = GlobalMessagePipe.CreateEvent<OnPyLlmProposalExportMessage>();
            //
            (_onPyServerStartPub, _onPyServerStartSub) = GlobalMessagePipe.CreateEvent<OnPyServerStartMessage>();
            (_onPyServerStopPub, _onPyServerStopSub) = GlobalMessagePipe.CreateEvent<OnPyServerStopMessage>();
            //
            _pyHandlers["PyCollect"] = (j) => OnPyCollect(j);
            _pyHandlers["PyExport"] = (j) => OnPyExport(j);
            _pyHandlers["PyLlmGenData"] = (j) => OnPyLlmGenData(j);
            _pyHandlers["PyLlmProposeModel"] = (j) => OnPyLlmProposeModel(j);
            _pyHandlers["PyLlmProposalExport"] = (j) => OnPyLlmProposalExport(j);
            _pyHandlers["PyConvertLlmThreadToAppDef"] = (j) => OnPyLlmProposalExport(j);
        }

        public void OnAppImagesInfoResp(GetAppImagesResponse resp)
        {
            _onImagesInfoPub.Publish(resp);
        }
        
        public void OnContainersInfoResp(GetAppContainersResponse resp)
        {
            _onContainersInfoPub.Publish(resp);
        }

        public void OnAppVersionsInfoResp(GetAppVersionsResponse resp)
        {
            _onAppVersionsInfoPub.Publish(resp);
        }

        public void OnAppEnvsInfoResp(GetAppEnvsResponse resp)
        {
            _onAppEnvsInfoPub.Publish(resp);
        }

        public void OnAppsInfoResp(GetWeb3AppsResponse resp)
        {
            _onAppsInfoPub.Publish(resp);
        }

        public void OnAppAIProposalsResp(GetAppAIProposalsResponse resp)
        {
            _onAppAIProposalsPub.Publish(resp);
        }

        public void OnJobInfoResp(GetJobResponse resp)
        {
            _onJobInfoPub.Publish(resp);
        }

        public void PublishServerHealthInfo(OnServerHealthInfoMsg msg)
        {
            _onServerHealthInfoPub.Publish(msg);
        }

        private void OnPyCollect(JObject jobj)
        {
            var msg = jobj.ToObject<OnPyCollectMessage>();
            _onPyCollectPub.Publish(msg);
        }

        private void OnPyExport(JObject jobj)
        {
            var msg = jobj.ToObject<OnPyExportMessage>();
            _onPyExportPub.Publish(msg);
        }

        private void OnPyServerStart(JObject jobj)
        {
            var msg = jobj.ToObject<OnPyServerStartMessage>();
            _onPyServerStartPub.Publish(msg);
        }

        private void OnPyServerStop(JObject jobj)
        {
            var msg = jobj.ToObject<OnPyServerStopMessage>();
            _onPyServerStopPub.Publish(msg);
        }

        private void OnPyLlmGenData(JObject jobj)
        {
            var msg = jobj.ToObject<OnPyLlmGenDataMessage>();
            _onPyLlmGenDataPub.Publish(msg);
        }

        private void OnPyLlmProposeModel(JObject jobj)
        {
            var msg = jobj.ToObject<OnPyLlmProposeModelMessage>();
            _onPyLlmProposeModelPub.Publish(msg);
        }

        private void OnPyLlmProposalExport(JObject jobj)
        {
            var msg = jobj.ToObject<OnPyLlmProposalExportMessage>();
            _onPyLlmProposalExportPub.Publish(msg);
        }

        public void PublishUniPipeMessage(string msg)
        {
            JObject msgObj = JObject.Parse(msg);
            var msgType = msgObj["Type"].ToString();
            Debug.Log($"UniPipe received: {msgType}");
            if (!_pyHandlers.ContainsKey(msgType))
            {
                return;
            }
            _pyHandlers[msgType](msgObj["Payload"].ToObject<JObject>());
            _onUniPipeMessagePub.Publish(msg);
        }
    }
}

