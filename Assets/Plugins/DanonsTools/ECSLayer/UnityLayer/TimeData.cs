using System;

namespace DanonsTools.ECSLayer.UnityLayer
{
    [Serializable]
    public struct TimeData
    {
        public float timeScale;
        public float time;
        public float renderDeltaTime;
        public float fixedDeltaTime;
        public float unscaledTime;
        public float unscaledRenderDeltaTime;
        public float unscaledFixedDeltaTime;
    }
}