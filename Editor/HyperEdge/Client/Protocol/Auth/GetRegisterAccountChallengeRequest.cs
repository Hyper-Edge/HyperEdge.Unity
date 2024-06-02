using System;
using MessagePack;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class GetRegisterAccountChallengeRequest
    {
        public string AddressHex { get; set; }
    }
}
