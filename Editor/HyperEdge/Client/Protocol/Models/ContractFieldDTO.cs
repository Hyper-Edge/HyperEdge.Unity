using MessagePack;


namespace HyperEdge.Shared.Protocol.Models
{
    [MessagePackObject(true)]
    public class ContractFieldDTO
    {
        public string Name { get; set; }
        public string Typename { get; set; }
        public string DefaultValue { get; set; }
    }
}
