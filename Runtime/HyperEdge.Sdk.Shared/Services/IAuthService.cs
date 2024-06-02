using MagicOnion;

using HyperEdge.Sdk.Shared.Protocol;


namespace HyperEdge.Sdk.Shared.Services
{
    public interface IAuthService : IService<IAuthService>
    {
        public UnaryResult<GetChallengeResponse> GetRegisterAccountChallenge(GetRegisterAccountChallengeRequest req);
        public UnaryResult<GetChallengeResponse> GetLoginChallenge(GetLoginChallengeRequest req);
        public UnaryResult<ValidateChallengeResponse> ValidateChallenge(ValidateChallengeRequest req);

    }
}
