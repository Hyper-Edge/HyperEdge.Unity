using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;


namespace HyperEdge.Sdk.Unity
{
    public class HeUserInfo
    {
        public string Id;
        public string AppId;
        public JObject Data;
    }

    public class GetAppUsersResponse
    {
        public List<HeUserInfo> Users { get; set; } = new();
    }
}

