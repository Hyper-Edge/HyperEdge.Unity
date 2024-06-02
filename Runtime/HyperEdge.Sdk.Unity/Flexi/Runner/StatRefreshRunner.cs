using System.Collections.Generic;
using static HyperEdge.Sdk.Unity.Flexi.AbilityFlowStepper;

namespace HyperEdge.Sdk.Unity.Flexi
{
    internal sealed class StatRefreshRunner
    {
        private readonly List<IAbilityFlow> flows = new List<IAbilityFlow>();

        internal void AddFlow(IAbilityFlow flow)
        {
            flows.Add(flow);
        }

        internal void Start()
        {
            for (var i = 0; i < flows.Count; i++)
            {
                IAbilityFlow flow = flows[i];

                bool keepRunning;
                do
                {
                    StepResult result = ExecuteStep(flow);
                    keepRunning = HandleStepResult(result);
                }
                while (keepRunning);
            }

            // Since no flow should be paused, every flows will be finished. Directly clear the list.
            Clear();
        }

        private bool HandleStepResult(StepResult result)
        {
            switch (result.type)
            {
                default:
                    return false;
                case ExecutionType.NODE_EXECUTION:
                    if (result.state != ResultState.SUCCESS)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
            }
        }

        internal void Clear()
        {
            flows.Clear();
        }
    }
}
