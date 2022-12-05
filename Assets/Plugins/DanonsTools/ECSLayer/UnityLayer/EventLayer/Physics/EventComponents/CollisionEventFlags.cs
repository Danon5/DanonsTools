using System;

namespace DanonsTools.ECSLayer.UnityLayer.EventLayer.Physics.EventComponents
{
    [Flags]
    public enum CollisionEventFlags
    {
        Enter,
        Stay,
        Exit
    }
}