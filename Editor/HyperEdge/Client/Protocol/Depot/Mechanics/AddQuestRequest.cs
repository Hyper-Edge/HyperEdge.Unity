using MessagePack;
using System;
using System.Collections.Generic;

using HyperEdge.Shared.Protocol.Models;
using HyperEdge.Shared.Protocol.Models.Mechanics;


namespace HyperEdge.Shared.Protocol
{
    [MessagePackObject(true)]
    public class AddQuestRequest
    {
        public Ulid AppId { get; set; }
        public QuestDTO Quest { get; set; }
    }
}
