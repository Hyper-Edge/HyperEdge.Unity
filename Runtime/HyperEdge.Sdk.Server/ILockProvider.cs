using System;
using System.Threading.Tasks;


namespace HyperEdge.Sdk.Server
{
    public interface ILockProvider
    {
        Task<ILockHandle> LockAsync(string key);
        ILockHandle Lock(string key);
    }
}
