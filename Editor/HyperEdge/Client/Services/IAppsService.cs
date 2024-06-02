using MagicOnion;

using HyperEdge.Shared.Protocol.Apps;


namespace HyperEdge.Shared.Services
{
    public interface IAppsService : IService<IAppsService>
    {
        public UnaryResult<CreateAppEnvResponse> CreateAppEnv(CreateAppEnvRequest req);
        public UnaryResult<BuildAppResponse> BuildApp(BuildAppRequest req);
        public UnaryResult<RunAppResponse> RunApp(RunAppRequest req);
    }
}
