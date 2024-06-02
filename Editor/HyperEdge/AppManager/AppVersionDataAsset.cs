using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

using HyperEdge.Shared.Protocol.Models.Export;


namespace HyperEdge.Sdk.Unity
{
    internal class AppVersionDataAsset : ScriptableObject
    {
        public Ulid Id { get; set; }
        public string Name { get; set; }
        public AppDefDTO AppDef { get; set; }

        public void LoadAppDefFromJsonFile(string filePath)
        {
            var appDefJson = File.ReadAllText(filePath);
            this.AppDef = JsonConvert.DeserializeObject<AppDefDTO>(appDefJson);
        }
    }
}
