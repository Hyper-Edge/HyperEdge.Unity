using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;


namespace HyperEdge.Sdk.Unity
{
    public class JsonPopupWindow : EditorWindow
    {
        private string _content = string.Empty;
        private Vector2 _scrollPosition;

        public static void ShowWindow(string title, JObject obj)
        {
            JsonPopupWindow window = ScriptableObject.CreateInstance<JsonPopupWindow>();
            window.titleContent = new GUIContent(title);
            window.SetJSON(obj);
            window.ShowUtility();
        }
        
        public void SetJSON(JObject obj)
        {
            _content = obj.ToString(Formatting.Indented);
        }

	private void OnGUI()
	{
	    EditorGUILayout.Space();
	    //
	    _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition,
               GUILayout.ExpandWidth(true),
               GUILayout.ExpandHeight(true),
               GUILayout.MaxHeight(400));

            EditorGUILayout.TextArea(_content,
            	GUILayout.ExpandWidth(true),
            	GUILayout.ExpandHeight(true));
            EditorGUILayout.Space();

	    EditorGUILayout.EndScrollView();
	    //
            EditorGUILayout.Space();
	}
    }
}

