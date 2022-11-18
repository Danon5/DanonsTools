using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DanonsTools.Ecs
{
    public sealed class DefaultCompositionDisposerService : ICompositionDisposerService
    {
        private readonly DestroyDetector _destroyDetector;

        public DefaultCompositionDisposerService()
        {
            _destroyDetector = new GameObject("DisposeServiceDestroyDetector").AddComponent<DestroyDetector>();
            Object.DontDestroyOnLoad(_destroyDetector);
        }
        
        public void AddPlayModeExitedListener(in Action listener)
        {
            _destroyDetector.DestroyEvent += listener;
        }

        public void RemovePlayModeExitedListener(in Action listener)
        {
            _destroyDetector.DestroyEvent -= listener;
        }
    }
}