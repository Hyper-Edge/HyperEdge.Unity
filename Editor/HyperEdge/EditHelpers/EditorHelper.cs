using System;
using System.Linq;
using UnityEngine;
using UnityEditor;

using HyperEdge.Shared.Protocol.Models.Mechanics;
using HyperEdge.Shared.Protocol.Models.Export;


namespace HyperEdge.Sdk.Unity.EditorHelpers
{
    public static class EditorHelper
    {
        public static RewardEditWindow OpenRewardEditor(RewardDTO reward, Action<RewardDTO> callback)
        {
            var rewardEditWnd = EditorWindow.GetWindow(typeof(RewardEditWindow), utility: true) as RewardEditWindow;
            rewardEditWnd.SetReward(reward);
            rewardEditWnd.Callback = callback;
            rewardEditWnd.Show();
            return rewardEditWnd;
        }
    }
}

