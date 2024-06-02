using System;
using System.Threading.Tasks;
using Grpc.Core;
using MagicOnion;
using MagicOnion.Client;
using UnityEngine;


namespace HyperEdge.Sdk.Unity.APITester
{
    class WithAppIdFilter : IClientFilter
    {
        private readonly ServerInfo _serverInfo;
        public WithAppIdFilter(ServerInfo serverInfo)
        {
            _serverInfo = serverInfo;
        }

        public async ValueTask<ResponseContext> SendAsync(RequestContext context, Func<RequestContext, ValueTask<ResponseContext>> next)
        {
            if (context.CallOptions.Headers.Get("ServerId") is null)
            {
                context.CallOptions.Headers.Add("ServerId", _serverInfo.ServerId);
            }
            return await next(context);
        }
    }
}
