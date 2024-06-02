using System;
using MessagePack;


namespace HyperEdge.Sdk.Shared.Protocol
{
    [MessagePackObject(true)]
    public class GetLoginChallengeRequest
    {
        public Ulid UserId { get; set; }
    }
}
