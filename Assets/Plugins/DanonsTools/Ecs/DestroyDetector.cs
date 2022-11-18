using System;
using JetBrains.Annotations;
using UnityEngine;

namespace DanonsTools.Ecs
{
    public sealed class DestroyDetector : MonoBehaviour
    {
        public Action DestroyEvent { get; set; }

        [UsedImplicitly]
        private void OnDestroy()
        {
            DestroyEvent?.Invoke();
        }
    }
}