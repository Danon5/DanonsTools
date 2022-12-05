using DG.Tweening;

namespace DanonsTools.UtilitiesLayer
{
    public static class TweenUtils
    {
        public static void SafeKill(this Tween tween)
        {
            if (tween != null && tween.IsActive())
                tween.Kill();
        }
    }
}