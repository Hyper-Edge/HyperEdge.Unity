using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;


namespace HyperEdge.Sdk.Unity
{
    public static class AssetUtils
    {
        public static List<T> FindAssetsByType<T>() where T : UnityEngine.Object
        {
	        var assets = new List<T>();
	        var assetGuids = AssetDatabase.FindAssets($"t:{typeof(T).FullName}");
            foreach(var assetGuid in assetGuids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if(asset is not null)
                {
                    assets.Add(asset);
                }
            }
            return assets;
        }

        public static T CreateNewAsset<T>(T obj, string assetName) where T : UnityEngine.Object
        {
            string folderPath = "Assets/HyperEdge";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets", "HyperEdge");
            }
            
            string assetFileName = GenerateUniqueAssetFileName(folderPath, assetName);
            string assetPath = Path.Combine(folderPath, assetFileName);

            AssetDatabase.CreateAsset(obj, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return obj;
        }

        public static string GenerateUniqueAssetFileName(string folderPath, string baseName)
        {
            string assetPath = Path.Combine(folderPath, $"{baseName}.asset");
            int counter = 1;

            // If the file exists, append a number to make the file name unique
            while (AssetDatabase.LoadAssetAtPath<AppData>(assetPath) != null)
            {
                assetPath = Path.Combine(folderPath, $"{baseName}_{counter}.asset");
                counter++;
            }

            return Path.GetFileName(assetPath);
        }
    }
}