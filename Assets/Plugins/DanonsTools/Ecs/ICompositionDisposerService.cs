using System;

namespace DanonsTools.Ecs
{
    public interface ICompositionDisposerService
    {
        public void AddPlayModeExitedListener(in Action listener);
        public void RemovePlayModeExitedListener(in Action listener);
    }
}