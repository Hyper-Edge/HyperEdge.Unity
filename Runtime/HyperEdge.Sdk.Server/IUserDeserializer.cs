using System;
using System.Reflection;


namespace HyperEdge.Sdk.Server
{
    public interface IUserDeserializer
    {
        public object Deserialize(UserPersistentData data);
    }
}
