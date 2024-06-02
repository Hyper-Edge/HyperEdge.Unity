using System;


namespace HyperEdge.Sdk.Server
{
    public interface IStorageModel
    {
        public Ulid Id { get; }
        public Ulid UserId { get; }
        public Ulid AppId { get; }
        public Ulid EnvId { get; }
        public string Type { get; }
        public string Data { get; }
    }
}
