using System;
using System.Collections.Generic;

using HyperEdge.Shared.Protocol.Models.Export;


namespace HyperEdge.Sdk.Unity
{
    public class OnPyLlmProposalExportMessage
    {
        public string AppId { get; set; }
        public string VersionId { get; set; }
        public string LlmProposalId { get; set; }
        public string AppDefFileId { get; set; }
    }
}

