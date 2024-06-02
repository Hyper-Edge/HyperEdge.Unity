using System.Collections.Generic;

namespace HyperEdge.Sdk.Unity.Flexi
{
    public class DefaultStatsRefreshAlgorithm : IStatsRefreshAlgorithm
    {
        private readonly List<IModifierHandler> handlers = new List<IModifierHandler>()
        {
            new AddendModifierHandler(),
            new MultiplierModifierHandler(),
        };

        public void RefreshStats(Actor actor)
        {
            for (var i = 0; i < handlers.Count; i++)
            {
                handlers[i].RefreshStats(actor.Owner);
            }
        }
    }
}
