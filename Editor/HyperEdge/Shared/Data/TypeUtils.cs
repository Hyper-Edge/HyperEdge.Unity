using UnityEngine;


namespace HyperEdge.Sdk.Unity
{
    public static class TypeUtils
    {
        public static string DefaultDataTypeValue(DataType dt)
        {
            switch (dt)
            {
                case DataType.String:
                    return "";
                case DataType.Integer:
                    return "0";
                case DataType.Float:
                    return  "0.0";
                case DataType.Boolean:
                    return "False";
                default:
                    return "";
            }
        }
    }
}
