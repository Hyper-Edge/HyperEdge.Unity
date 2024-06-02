using System;
using System.Collections.Generic;
using MessagePipe;
using Newtonsoft.Json;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEditor;


namespace HyperEdge.Sdk.Unity
{
    public class HyperEdgeWindowManager
    {
        public static void ShowWindow<T>() where T : EditorWindow
        {
            if (!string.IsNullOrEmpty(AppBuilderSettings.XApiKey))
            {
                var wnd = EditorWindow.GetWindow(typeof(T));
                wnd.Show();
            }
            else
            {
                EditorUtility.DisplayDialog("HyperEdge", "Set API Key in Settings, please.", "Ok");
            }
        }
    }
}
