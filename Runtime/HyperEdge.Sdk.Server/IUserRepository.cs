using System;
using System.Threading.Tasks;


namespace HyperEdge.Sdk.Server
{
    public interface IUserRepository
    {
        public Task<object> GetUserPersistentDataAsync(Ulid id);
        public Task<byte[]> GetUserCacheDataAsync(Ulid id);
        public Task SetUserCacheDataAsync(Ulid id, byte[] data);
        public Task RemoveUserCacheDataAsync(Ulid id);
        public Task CommitChanges(ChangeSet changeSet);
        //
        public object GetUserPersistentData(Ulid id);
        public byte[] GetUserCacheData(Ulid id);
        public void SetUserCacheData(Ulid id, byte[] data);
        public void RemoveUserCacheData(Ulid id);

    }
}
