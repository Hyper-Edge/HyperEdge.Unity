using System;
using System.Collections.Generic;


namespace HyperEdge.Sdk.Server
{
    public interface IGroup
    {
        Ulid Id { get; }
        Ulid AppId { get; }
        Ulid EnvId { get; }
        string ToJson();
        IEnumerable<Ulid> Members { get; }
    }
}
