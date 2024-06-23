using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEditor;

using HyperEdge.Shared.Protocol.Models.Mechanics;
using HyperEdge.Shared.Protocol.Models.Export;


namespace HyperEdge.Sdk.Unity.EditorHelpers
{
    public class RewardEditWindow : EditorWindow
    {
        private AppData? _appData = null;
        private AppDef? _appDef = null;

        private RewardEditHelper _rewardHelper = null;
        private RewardDTO _reward = null;

        public void SetReward(RewardDTO reward)
        {
            _reward = reward;
            _rewardHelper?.SetReward(reward);
        }

        public void Awake()
        {
            if (_appData is null)
            {
                _appData = AppDataManager.Default.CurrentAppData;
                if (_appData is null)
                {
                    return;
                }
            }
            if (_appDef is null)
            {
                var appDef = AppDefCache.Instance.GetCurrentAppDef(_appData.Name);
                if (!SetAppDef(appDef))
                {
                    return;
                }
            }
        }

        private bool SetAppDef(AppDef? appDef)
        {
            if (appDef is null)
            {
                return false;
            }
            _appDef = appDef;
            _rewardHelper = new RewardEditHelper(_appDef);
            if (_reward is not null)
            {
                _rewardHelper.SetReward(_reward);
            }
            return true;
        }

        private void OnGUI()
        {
            if (_appData is null)
            {
                return;
            }
            //
            if (_appDef is null)
            {
                return;
            }
            //
            _rewardHelper.RenderGUI(renderNameField: true);
        }
    }
}

