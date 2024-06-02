using MagicOnion;

using HyperEdge.Shared.Protocol;


namespace HyperEdge.Shared.Services
{
    public interface IAuthService : IService<IAuthService>
    {
        public UnaryResult<GetChallengeResponse> GetAuthChallenge(GetChallengeRequest req);
        public UnaryResult<ValidateChallengeResponse> ValidateAuthChallenge(ValidateChallengeRequest req);
        public UnaryResult<GetChallengeResponse> GetRegisterAccountChallenge(GetRegisterAccountChallengeRequest req);
        public UnaryResult<RegisterAccountResponse> RegisterAccount(RegisterAccountRequest req);
        public UnaryResult<UpdateAccountResponse> UpdateAccount(UpdateAccountRequest req);
        public UnaryResult<GetAccountResponse> GetAccount(GetAccountRequest req);
    
        public UnaryResult<CreateApiKeyResponse> CreateApiKey(CreateApiKeyRequest req);
        public UnaryResult<DeleteApiKeyResponse> DeleteApiKey(DeleteApiKeyRequest req);
    }
}
