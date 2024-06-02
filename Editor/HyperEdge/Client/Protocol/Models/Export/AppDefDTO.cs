using System;
using System.Collections.Generic;
using MessagePack;

using HyperEdge.Shared.Protocol.Models.Mechanics;


namespace HyperEdge.Shared.Protocol.Models.Export
{
    [MessagePackObject(true)]
    public class AppDefDTO
    {
        public Ulid AppId { get; set; }
        public string Name { get; set; }
        public List<DataClassDTO> DataClasses { get; set; }
        public List<DataClassDTO> UGCDataClasses { get; set; }
        public List<DataClassDTO> ModelClasses { get; set; }
        public List<DataClassDTO> StructClasses { get; set; }
        public List<DataClassDTO> EventClasses { get; set; }
        public List<Tuple<int, DataClassDTO>> StorageClasses { get; set; }
        public List<UserGroupClassDTO> GroupClasses { get; set; }
        public Dictionary<string, List<DataClassInstanceDTO>> DataClassInstances { get; set; }
        public List<InventoryDefDTO> Inventories { get; set; }
        //
        public List<CraftRulesDTO> CraftRules { get; set; }
        public List<QuestDTO> Quests { get; set; }
        public List<ProgressionSystemDTO> Progressions { get; set; }
        public List<GenericLadderDTO> ProgressionLadders { get; set; }
        public List<BattlePassDTO> BattlePasses { get; set; }
        public List<BattlePassInstanceDTO> BattlePassInstances { get; set; }
        public List<TournamentDTO> Tournaments { get; set; }
        public List<RewardDTO> Rewards { get; set; }
        public List<EnergySystemDTO> EnergySystems { get; set; }
        public List<ReqHandlerDTO> RequestHandlers { get; set; }
        public List<JobHandlerDTO> JobHandlers { get; set; }
        public List<EventHandlerDTO> EventHandlers { get; set; }
        //
        public List<AbilitySystemDTO> AbilitySystems { get; set; }
        public List<AbilityGraphDTO> AbilityGraphs { get; set; }
        public List<AbilityNodeDTO> AbilityNodes { get; set; }
        //
        public List<MessageRelaySystemDTO> MsgRelaySystems { get; set; }
        public List<TurnGameSystemDTO> TurnGameSystems { get; set; }
        //
        public List<NetEntityDTO> NetEntities { get; set; }
        public List<MultiPlayerSystemDTO> MultiPlayerSystems { get; set; }
    }
}
