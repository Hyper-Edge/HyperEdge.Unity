using System;
using MessagePack;


namespace HyperEdge.Sdk.Shared.Protocol
{
    [MessagePackObject(true)]
    public class GetRegisterAccountChallengeRequest
    {
        public string AddressHex { get; set; }
    }
}
