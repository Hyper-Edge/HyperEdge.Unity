using MagicOnion;

using HyperEdge.Sdk.Shared.Protocol;


namespace HyperEdge.Sdk.Shared.Services
{
    public interface IAccountService : IService<IAccountService>
    {
        public UnaryResult<RegisterAccountResponse> RegisterAccount(RegisterAccountRequest req);
        public UnaryResult<UpdateAccountResponse> UpdateAccount(UpdateAccountRequest req);
    }
}
