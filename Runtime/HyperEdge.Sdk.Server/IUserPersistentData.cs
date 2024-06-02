using System;
using System.Collections.Generic;


namespace HyperEdge.Sdk.Server
{
    public class UserPersistentData
    {
        public Ulid Id { get; set; }
        public Ulid EnvId { get; set; }
        public string AddressHex { get; set; }
        public List<IStorageModel> Storage { get; set; }
        public List<IUserInventoryModel> Inventory { get; set; }
        public string Data { get; set; }
    }
}
