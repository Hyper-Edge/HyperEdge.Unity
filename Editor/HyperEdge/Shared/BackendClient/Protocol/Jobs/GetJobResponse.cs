using System;
using Newtonsoft.Json.Linq;


namespace HyperEdge.Sdk.Unity
{
    public class GetJobResponse
    {
        public string Id = string.Empty;
        public bool Complete = false;
        public JObject Data = null;

        public bool IsCompleted()
        {
            return Data.ContainsKey("status");
        }
    }
}

