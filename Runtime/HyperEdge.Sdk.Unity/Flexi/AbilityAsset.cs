using System;
using System.Collections.Generic;
using UnityEngine;

namespace HyperEdge.Sdk.Unity.Flexi
{
    /// <summary>
    /// An AbilityAsset preserves an ability data. This asset is only used for Unity serialization.
    /// It can create <see cref="AbilityData"/> for runtime usage.
    /// </summary>
    [CreateAssetMenu(fileName = "NewAbilityAsset", menuName = "HyperEdge/Flexi/Ability Asset", order = 151)]
    public sealed class AbilityAsset : GraphAsset
    {
        [SerializeField]
        private List<BlackboardVariable> blackboard = new List<BlackboardVariable>();
        [Obsolete]
        [HideInInspector]
        [SerializeField]
        private List<string> graphJsons = new List<string>();
        [HideInInspector]
        [SerializeField]
        private List<AbilityGraphGroup> graphGroups = new List<AbilityGraphGroup>();

        [NonSerialized]
        private AbilityData abilityData;

        internal List<BlackboardVariable> Blackboard
        {
            get
            {
                return blackboard;
            }
            set
            {
                blackboard.Clear();
                if (value != null)
                {
                    // Clone each variable to prevent modify the source
                    for (var i = 0; i < value.Count; i++)
                    {
                        blackboard.Add(value[i].Clone());
                    }
                }
            }
        }

        [Obsolete]
        internal List<string> GraphJsons => graphJsons;
        internal List<AbilityGraphGroup> GraphGroups => graphGroups;

        public AbilityData Data
        {
            get
            {
                if (abilityData == null)
                {
                    abilityData = CreateAbilityData(this);
                }

                return abilityData;
            }
        }

        private static AbilityData CreateAbilityData(AbilityAsset abilityAsset)
        {
            var abilityData = new AbilityData { name = abilityAsset.name };
            for (var i = 0; i < abilityAsset.blackboard.Count; i++)
            {
                abilityData.blackboard.Add(abilityAsset.blackboard[i].Clone());
            }

            for (var i = 0; i < abilityAsset.graphGroups.Count; i++)
            {
                abilityData.graphGroups.Add(abilityAsset.graphGroups[i].Clone());
            }

            return abilityData;
        }
    }
}
