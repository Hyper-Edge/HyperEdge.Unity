using UnityEditor;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;


namespace HyperEdge.Sdk.Unity
{
    public class JsonPopupWindow : EditorWindow
    {
        private static List<string> jsonContents;
        private Vector2 scrollPosition;

        public static void ShowWindow(List<string> contents)
        {
            JsonPopupWindow window = ScriptableObject.CreateInstance<JsonPopupWindow>();
            window.titleContent = new GUIContent("JSON Objects");
            jsonContents = contents;
            window.ShowUtility();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("JSON Objects:", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            foreach (var jsonContent in jsonContents)
            {
                EditorGUILayout.TextArea(jsonContent, GUILayout.ExpandWidth(true));
                EditorGUILayout.Space();
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            if (GUILayout.Button("Close"))
            {
                this.Close();
            }
        }
    }
}
