using System.Collections.Generic;
using UnityEngine;

namespace DanonsTools.InspectorLayer
{
    public sealed class AnimationWindowClipSource : MonoBehaviour, IAnimationClipSource
    {
        [SerializeField] private List<AnimationClip> _clips;

        public void GetAnimationClips(List<AnimationClip> results)
        {
            foreach (var clip in _clips)
                results.Add(clip);
        }
    }
}