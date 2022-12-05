using UnityEngine;

namespace DanonsTools.UtilitiesLayer
{
    public static class Math2DUtilities
    {
        public static Vector3 RotatedOrthographicScreenToWorldPoint(this Camera camera, in Vector2 screenPoint)
        {
            var mouseRay = camera.ScreenPointToRay(screenPoint);
            var plane = new Plane(Vector3.back, Vector3.zero);
            plane.Raycast(mouseRay, out var dist);
            return mouseRay.origin + mouseRay.direction * dist;
        }
    }
}