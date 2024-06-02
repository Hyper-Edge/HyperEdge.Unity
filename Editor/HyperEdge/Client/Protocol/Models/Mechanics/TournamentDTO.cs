using System;
using System.Collections.Generic;
using MessagePack;

using HyperEdge.Shared.Protocol.Models.Export;


namespace HyperEdge.Shared.Protocol.Models.Mechanics
{
    [MessagePackObject(true)]
    public class TournamentDTO
    {
        public Ulid Id { get; set; }
        public string Name { get; set; }
        public string ScoreFieldName { get; set; }
        public DataClassFieldsDTO Model { get; set; }
        public DataClassFieldsDTO Data { get; set; }
    }
}
