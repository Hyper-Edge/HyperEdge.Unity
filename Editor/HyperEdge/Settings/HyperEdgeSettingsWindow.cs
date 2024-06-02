using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using MessagePipe;
using Newtonsoft.Json;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEditor;

using HyperEdge.Shared.Protocol.Models;
using HyperEdge.Shared.Protocol.Models.Export;


namespace HyperEdge.Sdk.Unity
{
    public class HyperEdgeSettingsWindow : EditorWindow
    {
        private int _toolbarIdx = 0;
        private string[] _toolbarStrings = {
            "API Key Settings"
        };

        private enum TabIndices
        {
            API_KEY_TAB_IDX = 0
        };

        private string _newAPIKey = string.Empty;

        private HyperEdgePy? _py = null;
        private HyperEdgeBackendClient _heClient = null;

        private IDisposable _bag = null;

        [MenuItem("HyperEdge/Settings")]
        public static void ShowWindow()
        {
            var wnd = GetWindow(typeof(HyperEdgeSettingsWindow));
            wnd.Show();
        }

        public void Awake()
        {
            InitCallbacks();
            if (_heClient is null)
            {
                _heClient = new HyperEdgeBackendClient();
            }
            if (_py is null)
            {
                _py = new HyperEdgePy("");
            }

            if (!string.IsNullOrEmpty(AppBuilderSettings.XApiKey))
            {
                _newAPIKey = AppBuilderSettings.XApiKey;
                CheckAPIKey().Forget();
            }
        }

        public void Destroy()
        {
            _bag.Dispose();
        }

        private void InitCallbacks()
        {
            if (_bag is null)
            {
                var d = DisposableBag.CreateBuilder();
                _bag = d.Build();
            }
        }

        private void OnGUI()
        {
            _toolbarIdx = GUILayout.Toolbar(_toolbarIdx, _toolbarStrings);
            if (_toolbarIdx == (int)TabIndices.API_KEY_TAB_IDX)
            {
                OnApiKeyTab();
            }
        }

        private void OnApiKeyTab()
        {
            GUILayout.BeginVertical();
            EditorGUILayout.LabelField($"Logged as: {APIKeyManager.Instance.UserId}");
            //
            EditorGUILayout.LabelField("API Key:");
            _newAPIKey = EditorGUILayout.TextField(_newAPIKey);
            //
            GUI.enabled = !string.IsNullOrEmpty(_newAPIKey);
            if (GUILayout.Button("Check API Key"))
            {
                CheckAPIKey().Forget();
            }
            GUI.enabled = true;
            //
            GUILayout.EndVertical();
        }

        private async UniTask CheckAPIKey()
        {
            if (string.IsNullOrEmpty(_newAPIKey))
            {
                return;
            }
            var success = await APIKeyManager.Instance.CheckAPIKey(_newAPIKey);
            if (success)
            {
                EditorUtility.DisplayDialog("HyperEdge", "API Key Check Success", "Ok");
            }
            else
            {
                EditorUtility.DisplayDialog("HyperEdge", "API Key Check Failed", "Ok");
            }
        }
    }
}

