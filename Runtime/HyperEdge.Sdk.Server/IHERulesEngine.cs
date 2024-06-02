using System;
using System.Threading.Tasks;


namespace HyperEdge.Sdk.Server
{
    public interface IHERulesEngine
    {
        ValueTask<bool> ExecuteRulesAsync(string workflowId, params object[] _params);
    }
}
