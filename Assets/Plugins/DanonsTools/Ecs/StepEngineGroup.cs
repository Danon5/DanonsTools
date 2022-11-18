using System;
using Svelto.DataStructures;
using Svelto.ECS;

namespace DanonsTools.Ecs
{
    [AllowMultiple]
    public sealed class StepEngineGroup : UnsortedEnginesGroup<IStepEngine<float>, float>
    {
        public StepEngineGroup(in FasterList<IStepEngine<float>> engines, ref Action<float> tickAction) : base(engines)
        {
            tickAction += Step;
        }

        private void Step(float deltaTime)
        {
            base.Step(deltaTime);
        }
    }
}