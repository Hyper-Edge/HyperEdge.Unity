using System;


namespace HyperEdge.Sdk.Server
{
    public interface IUserInventoryModel
    {
        public Ulid UserId { get; }
        public Ulid ItemId { get; }
        public ulong Amount { get; }
    }
}
