using System;


namespace HyperEdge.Sdk.Server
{
    public interface IModel<TKey>
    {
        TKey Id { get; }
        Ulid UserId { get; }
        Ulid AppId { get; }
        Ulid EnvId { get; }
        ulong TokenId { get; }
        string ToJson();
    }
}
