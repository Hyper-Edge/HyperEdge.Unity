using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;


namespace HyperEdge.Sdk.Unity.CodeEditor
{
    public class DocsViewWindow : EditorWindow
    {
        [MenuItem("HyperEdge/Documentation/Plugin")]
        static void OpenPluginDocs()
        {
            WebWindow.Show("HyperEdge Documentation", "https://docs.hyperedgelabs.xyz");
        }

        [MenuItem("HyperEdge/Documentation/Project")]
        static void OpenCurrentVersionDocs()
        {
            var appData = AppDataManager.Default.CurrentAppData;
            if (appData is null)
            {
                EditorUtility.DisplayDialog("HyperEdge", "Load Application first.", "Ok");
                return;
            }
            var docfxUrl = "https://" + HyperEdgeConstants.BackendUrl + $"/docfx/apps/{appData.Id}/index.html";
            WebWindow.Show("HyperEdge Documentation", docfxUrl);
        }
    }
}

