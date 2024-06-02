using System;
using System.Collections.Generic;


namespace HyperEdge.Sdk.Unity.CoDesigner
{
    public class LlmUserActionProposalInfo
    {
        public string ActionName { get; set; } = string.Empty;
        public List<string> TargetItems = new();
        public string ActionResult = string.Empty;
        public bool IsUpgradeOrLevelUp = false;
        public bool IsPurchaseAction = false;
        public bool IsCraftingOrProduceAction = false;
        public bool IsRetireOrSellAction = false;
    }
}

