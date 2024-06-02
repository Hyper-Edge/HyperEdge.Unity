using MessagePack;


namespace HyperEdge.Shared.Protocol.Models
{
    [MessagePackObject(true)]
    public class DataClassItemFieldDTO
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
