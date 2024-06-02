using System;
using MessagePack;

using HyperEdge.Sdk.Shared.Protocol.Models;


namespace HyperEdge.Sdk.Shared.Protocol
{
    [MessagePackObject(true)]    
    public class RegisterAccountResponse
    {
        public PlayerAccountDTO Account { get; set; }        
        public bool Success { get; set; }
        public Ulid UserId { get; set; }
    }
}
