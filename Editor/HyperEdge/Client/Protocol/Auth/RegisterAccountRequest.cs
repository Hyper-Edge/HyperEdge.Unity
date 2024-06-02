using System;
using MessagePack;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class RegisterAccountRequest
    {
        public string AddressHex { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Signature { get; set; } = string.Empty;
    }
}
