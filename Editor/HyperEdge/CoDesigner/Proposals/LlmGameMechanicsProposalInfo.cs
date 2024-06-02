using System;
using System.Collections.Generic;


namespace HyperEdge.Sdk.Unity.CoDesigner
{
    public class LlmGameMechanicInfo
    {
        public string Name = string.Empty;
        public string Description = string.Empty;
    }

    public class LlmGameMechanicsProposalInfo
    {
        public List<LlmGameMechanicInfo> GameMechanics = new();
    }
}

