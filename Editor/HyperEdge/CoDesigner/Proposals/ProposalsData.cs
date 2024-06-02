using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using UnityEngine;


namespace HyperEdge.Sdk.Unity.CoDesigner
{
    public class ProposalsData
    {
        public string Id = string.Empty;
        public string VersionId = string.Empty;
        public List<LlmUserActionProposalInfo> UserActions = new();
        public List<LlmCurrencyProposalInfo> CurrencyProposals = new();
        public List<LlmResourcesProposalInfo> ResourcesProposals = new();
        public List<LlmMiniGameProposalInfo> MiniGameProposals = new();
        public List<LlmModelProposalInfo> ModelProposals = new();

        public static ProposalsData LoadFromJObject(JObject obj)
        {
            var data = new ProposalsData();
            foreach(var el in obj["AIProposals"])
            {
                var elType = el["Type"].ToObject<string>();
                var elData = el["Data"];
                if (elType == "LlmUserActionProposalInfo")
                {
                    data.UserActions.Add(elData.ToObject<LlmUserActionProposalInfo>());
                }
                else if (elType == "LlmCurrencyProposalInfo")
                {
                    data.CurrencyProposals.Add(elData.ToObject<LlmCurrencyProposalInfo>());
                }
                else if (elType == "LlmResourceProposalInfo")
                {
                    data.ResourcesProposals.Add(elData.ToObject<LlmResourcesProposalInfo>());
                }
                else if (elType == "LlmMiniGameProposalInfo")
                {
                    data.MiniGameProposals.Add(elData.ToObject<LlmMiniGameProposalInfo>());
                }
                else if (elType == "LlmModelProposalInfo")
                {
                    data.ModelProposals.Add(elData.ToObject<LlmModelProposalInfo>());
                }
            }
            return data;
        }

        public static ProposalsData LoadFromJsonFile(string fpath)
        {
            var text = File.ReadAllText(fpath);
            var obj = JsonConvert.DeserializeObject<JObject>(text);
            return LoadFromJObject(obj);
        }
    }
}

