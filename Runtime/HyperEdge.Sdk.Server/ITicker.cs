using System;


namespace HyperEdge.Sdk.Server
{
    public interface ITicker
    {
        int Id { get; }
        ulong Value { get; }
        ulong MaxCapacity { get; }
        ulong RegenValue { get; }
        ulong RegenRate { get; }
        DateTime LastUpdated { get; }
        //
        ulong Update();
    }
}
