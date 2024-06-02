using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;


namespace HyperEdge.Sdk.Unity
{
    public class GDDecomposeRequest
    {
        public string AppId { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        //
        public bool ProposeEnergySystems { get; set; } = false;
        public bool ProposeAcheivementTypes { get; set; } = false;
        public bool ProposeQuestTypes { get; set; } = false;
        public bool ProposeSocialMechanics { get; set; } = false;
    }
}

