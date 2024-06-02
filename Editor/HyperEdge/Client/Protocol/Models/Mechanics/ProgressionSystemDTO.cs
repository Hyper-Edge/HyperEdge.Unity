using System;
using System.Collections.Generic;
using MessagePack;

using HyperEdge.Shared.Protocol.Models.Export;


namespace HyperEdge.Shared.Protocol.Models.Mechanics
{
    [MessagePackObject(true)]
    public class ProgressionSystemDTO
    {
        public Ulid Id { get; set; }
        public string EntityName { get; set; }
        public bool IsExperienceBased { get; set; }
        public string LevelField { get; set; }
        public string ExperienceField { get; set; }
        public DataClassFieldsDTO LadderLevelData { get; set; }
    }
}
