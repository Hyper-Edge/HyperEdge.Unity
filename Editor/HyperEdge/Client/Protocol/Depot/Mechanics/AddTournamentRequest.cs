using MessagePack;
using System;
using System.Collections.Generic;

using HyperEdge.Shared.Protocol.Models;
using HyperEdge.Shared.Protocol.Models.Mechanics;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class AddTournamentRequest
    {
        public Ulid AppId { get; set; }
        public TournamentDTO Tournament { get; set; }
    }
}
