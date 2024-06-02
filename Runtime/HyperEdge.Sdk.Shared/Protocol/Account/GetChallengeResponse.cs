using MessagePack;


namespace HyperEdge.Sdk.Shared.Protocol
{
    [MessagePackObject(true)]
    public class GetChallengeResponse
    {
        public string Challenge { get; set; }
    }
}
