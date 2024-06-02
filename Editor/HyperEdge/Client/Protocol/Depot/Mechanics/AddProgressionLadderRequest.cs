using MessagePack;
using System;
using System.Collections.Generic;

using HyperEdge.Shared.Protocol.Models;
using HyperEdge.Shared.Protocol.Models.Mechanics;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class AddProgressionLadderRequest
    {
        public enum Type
        {
            GENERAL = 0,
            BATTLE_PASS = 1
        }

        public Ulid AppId { get; set; }
        public int LadderType { get; set; }
        public GenericLadderDTO Ladder { get; set; }
    }
}
