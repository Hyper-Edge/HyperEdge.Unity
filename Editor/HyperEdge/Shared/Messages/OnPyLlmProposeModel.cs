using System;
using System.Collections.Generic;

using HyperEdge.Shared.Protocol.Models.Export;


namespace HyperEdge.Sdk.Unity
{
    public class OnPyLlmProposeModelMessage
    {
        public bool Success { get; set; }
        public List<DataClassInstanceFieldDTO> Data { get; set; } = new();
    }
}

