using System;
using static HyperEdge.Sdk.Unity.Flexi.AbilityFlowStepper;

namespace HyperEdge.Sdk.Unity.Flexi
{
    public abstract class AbilityFlowRunner
    {
        public enum RunningState
        {
            IDLE, RUNNING, PAUSE
        }

        public enum EventTriggerMode
        {
            EACH_NODE,
            EACH_FLOW,
            NEVER,
        }

        internal event Action<StepResult> StepExecuted;

        internal AbilitySystem abilitySystem;

        protected EventTriggerMode eventTriggerMode = EventTriggerMode.EACH_NODE;


        internal void SetEventTriggerMode(EventTriggerMode eventTriggerMode)
        {
            this.eventTriggerMode = eventTriggerMode;
        }

        public abstract void AddFlow(IAbilityFlow flow);
        public abstract bool IsFlowRunning(IAbilityFlow flow);

        public abstract void Start();
        public abstract void Resume(IResumeContext resumeContext);
        public abstract void Tick();

        public virtual void BeforeTriggerEvents()
        {

        }

        public virtual void AfterTriggerEvents()
        {

        }

        public virtual void Clear()
        {

        }

        protected void NotifyStepResult(StepResult result)
        {
            StepExecuted?.Invoke(result);
        }
    }
}
