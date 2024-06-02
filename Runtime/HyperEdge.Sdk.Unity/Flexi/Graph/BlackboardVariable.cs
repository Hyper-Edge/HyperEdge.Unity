using System;
using MessagePack;


namespace HyperEdge.Sdk.Unity.Flexi
{
    [MessagePackObject(true)]
    public class BlackboardVariable
    {
        public string key = "";
        public int value;

        public BlackboardVariable Clone()
        {
            return new BlackboardVariable
            {
                key = key,
                value = value,
            };
        }
    }
}
