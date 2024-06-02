using System;
using System.Collections.Generic;
using MessagePack;

using HyperEdge.Shared.Protocol.Models;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class GetDataClassContractsResponse
    {
        public List<DataClassContractDTO> DataClassContracts { get; set; }
    }
}
