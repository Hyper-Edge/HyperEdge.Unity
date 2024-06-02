using UnityEditor;


namespace HyperEdge.Sdk.Unity
{
    public class AppBuilderSettings
    {
        const string X_API_KEY_PREF_KEY = "HyperEdge.XApiKey";

        public static string XApiKey
        {
            get => EditorPrefs.GetString(X_API_KEY_PREF_KEY);
            set => EditorPrefs.SetString(X_API_KEY_PREF_KEY, value);
        }
    }    
}
