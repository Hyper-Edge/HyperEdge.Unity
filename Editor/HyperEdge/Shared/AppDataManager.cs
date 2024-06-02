using System;
using System.Collections.Generic;
using UnityEngine;


namespace HyperEdge.Sdk.Unity
{
    public class AppDataManager
    {
        private static AppDataManager _default_instance = new AppDataManager();
        public static AppDataManager Default { get => _default_instance; }

        private AppData? _currentAppData = null;
        public AppData? CurrentAppData { get => _currentAppData; }
        public void SetAppData(AppData appData)
        {
            _currentAppData = appData;
        }
    }
}

