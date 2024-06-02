using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;


namespace HyperEdge.Sdk.Unity.CodeEditor
{
    public class CodeEditorWindow : EditorWindow
    {
        [MenuItem("HyperEdge/CodeEditor/Open")]
        static void Open()
        {
            WebWindow.Show("HyperEdge", "http://localhost:5001");
        }

        public void Awake()
        {
        }

        private void OnGUI()
        {
        }
    }
}

