using MagicOnion;

using HyperEdge.Sdk.Shared.Protocol;


namespace HyperEdge.Sdk.Shared.Services
{
    public interface IHealthService : IService<IHealthService>
    {
        public UnaryResult<CheckHealthResponse> CheckHealth();
    }
}
