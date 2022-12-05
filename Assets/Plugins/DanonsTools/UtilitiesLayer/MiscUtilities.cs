using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Color = UnityEngine.Color;

namespace DanonsTools.UtilitiesLayer
{
    public static class MiscUtilities
    {
        private static Camera mainCam;

        public static void DebugAll<TElement, TLogValue>(this IEnumerable<TElement> list, Func<TElement, TLogValue> logValGetter)
        {
            foreach (var element in list)
                Debug.Log(logValGetter(element));
        }
        
        public static float GetFloatListSum(List<float> list)
        {
            float sum = 0f;
            foreach (float val in list)
                sum += val;
            return sum;
        }

        public static int GetIntListSum(List<int> list)
        {
            int sum = 0;
            foreach (int val in list)
                sum += val;
            return sum;
        }

        public static Vector3 Abs(this Vector3 vec)
        {
            return new Vector3(Mathf.Abs(vec.x), Mathf.Abs(vec.y), Mathf.Abs(vec.z));
        }

        public static Vector2 Abs(this Vector2 vec)
        {
            return new Vector2(Mathf.Abs(vec.x), Mathf.Abs(vec.y));
        }
        
        public static bool TryRaycastAlongLine(Vector3 from, Vector3 to, out RaycastHit hit, LayerMask layerMask)
        {
            Vector3 dir = to - from;
            float dist = dir.magnitude;
            dir.Normalize();

            return Physics.Raycast(from, dir, out hit, dist, layerMask);
        }
        
        public static bool TryRaycastAlongLine(Vector3 from, Vector3 to, out RaycastHit hit)
        {
            Vector3 dir = to - from;
            float dist = dir.magnitude;
            dir.Normalize();

            return Physics.Raycast(from, dir, out hit, dist);
        }
        
        public static bool TrySpherecastAlongLine(Vector3 from, Vector3 to, float radius, 
            out RaycastHit hit, LayerMask layerMask)
        {
            Vector3 dir = to - from;
            float dist = dir.magnitude;
            dir.Normalize();

            return Physics.SphereCast(from, radius, dir, out hit, dist, layerMask);
        }
        
        public static bool TrySpherecastAlongLine(Vector3 from, Vector3 to, float radius, out RaycastHit hit)
        {
            Vector3 dir = to - from;
            float dist = dir.magnitude;
            dir.Normalize();

            return Physics.SphereCast(from, radius, dir, out hit, dist);
        }
        
        public static bool TryGetClosestElementInCollection<T>(in Vector3 position, List<T> collection, 
            out T closestElement, float radius = Mathf.Infinity) where T : MonoBehaviour
        {
            closestElement = default;
            
            if (collection.Count == 0) return false;
            
            float closestDist = Mathf.Infinity;
            
            foreach (T element in collection)
            {
                if (element == null) continue;
                if (element.transform == null) continue;

                float dist = Vector3.Distance(position, element.transform.position);

                if (dist > radius) continue;
                
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestElement = element;
                }
            }

            return closestElement != null;
        }
        
        public static Vector2 WorldMousePos
        {
            get
            {
                if (mainCam == null)
                    mainCam = Camera.main;

                return mainCam.ScreenToWorldPoint(Input.mousePosition);
            }
        }

        public static Vector2 ProjectOnNormal(Vector2 vector, Vector2 normal)
        {
            return Vector3.ProjectOnPlane(vector, normal);
        }
        
        public static Vector3 RepairHitSurfaceNormal(RaycastHit hit, int layerMask)
        {
            if(hit.collider is MeshCollider collider)
            {
                Mesh mesh = collider.sharedMesh;
                int[] tris = mesh.triangles;
                Vector3[] verts = mesh.vertices;
 
                Vector3 v0 = verts[tris[hit.triangleIndex * 3]];
                Vector3 v1 = verts[tris[hit.triangleIndex * 3 + 1]];
                Vector3 v2 = verts[tris[hit.triangleIndex * 3 + 2]];
 
                Vector3 n = Vector3.Cross(v1 - v0, v2 - v1).normalized;
 
                return hit.transform.TransformDirection(n);
            }

            Vector3 p = hit.point + hit.normal * 0.01f;
            Physics.Raycast(p, -hit.normal, out hit, 0.011f, layerMask);
            return hit.normal;
        }

        public static float RoundToNumber(float value, float number)
        {
            return Mathf.RoundToInt(value / number) * number;
        }

        public static Vector3 DirectionToEuler(Vector3 direction)
        {
            Quaternion rot = Quaternion.LookRotation(direction);
            return rot.eulerAngles;
        }

        public static Vector3 EulerToDirection(Vector3 euler)
        {
            return Quaternion.Euler(euler) * Vector3.forward;
        }

        public static Vector3Int ToVec3Int(this Vector3 vec, bool round = true)
        {
            if (!round)
                return new Vector3Int(
                    (int)vec.x,
                    (int)vec.y,
                    (int)vec.z);
            return new Vector3Int(
                Mathf.RoundToInt(vec.x), 
                Mathf.RoundToInt(vec.y), 
                Mathf.RoundToInt(vec.z));
        }

        public static byte ByteClamp(byte value, byte min, byte max)
        {
            if (value < min)
                return min;
            return value > max ? max : value;
        }

        public static float Remap(this float from, float fromMin, float fromMax, float toMin, float toMax)
        {
            float fromAbs = from - fromMin;
            float fromMaxAbs = fromMax - fromMin;

            float normal = fromAbs / fromMaxAbs;

            float toMaxAbs = toMax - toMin;
            float toAbs = toMaxAbs * normal;

            float to = toAbs + toMin;

            return to;
        }

        public static float Remap01(this float from, float fromMin, float fromMax)
        {
            return Remap(from, fromMin, fromMax, 0f, 1f);
        }

        public static Vector3 ToVector3(this Color col)
        {
            return new Vector3(col.r, col.g, col.b);
        }

        public static Color ToColor(this Vector3 vec)
        {
            return new Color(vec.x, vec.y, vec.z);
        }

        public static string UppercaseFirst(this string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        public static T GetRandomElementOfList<T>(this List<T> list)
        {
            return list[Random.Range(0, list.Count)];
        }
        public static T GetRandomElementOfArray<T>(this T[] array)
        {
            return array[Random.Range(0, array.Length)];
        }

        public static bool IsCharAVowel(this char c)
        {
            return "aeiouAEIOU".IndexOf(c) >= 0;
        }

        public static T GetRandomEnum<T>(params int[] excludedIndices) where T : struct, IConvertible
        {
            Type enumType = typeof(T);
            if (!enumType.IsEnum)
                throw new ArgumentException("Not an enum");

            var enumVals = Enum.GetValues(enumType).Cast<int>().ToList();

            enumVals.RemoveAll(excludedIndices.Contains);

            return (T) (object) enumVals[Random.Range(0, enumVals.Count)];
        }


        public static bool GenericTryParse<T>(this string input, out T value)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));

            if (converter.IsValid(input))
            {
                value = (T) converter.ConvertFromString(input);
                return true;
            }

            value = default;
            return false;
        }

        public static T Convert<T>(this string input)
        {
            try
            {
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
                return (T) converter.ConvertFromString(input);
            }
            catch (NotSupportedException)
            {
                return default;
            }
        }

        public static bool TryCast<T>(this object obj, out T result)
        {
            if (obj is T obj1)
            {
                result = obj1;
                return true;
            }

            result = default;
            return false;
        }

        public static bool RunProbability(float chance)
        {
            float random = Random.Range(0f, 1f);

            return random < chance;
        }

        public static Vector3 SnapToPPU(Vector3 unsnappedPos, int PPU)
        {
            float PPUSnap = 1f / PPU;

            Vector3 pos = unsnappedPos;
            pos.x = Mathf.Round(pos.x / PPUSnap) * PPUSnap;
            pos.y = Mathf.Round(pos.y / PPUSnap) * PPUSnap;
            return pos;
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            for (int i = list.Count; i > 0; i--)
                list.Swap(0, Random.Range(0, i));
        }

        public static void Swap<T>(this IList<T> list, int i, int j)
        {
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }

        public static double Normalize(double value, double start, double end)
        {
            double width = end - start;
            double offsetValue = value - start;

            return offsetValue - Mathf.Floor((float) offsetValue / (float) width) * width + start;
        }

        public static Vector3 LerpEuler(Vector3 a, Vector3 b, float t)
        {
            return new Vector3(
                Mathf.LerpAngle(a.x, b.x, t),
                Mathf.LerpAngle(a.y, b.y, t),
                Mathf.LerpAngle(a.z, b.z, t));
        }


        public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
        {
            return Quaternion.Euler(angles) * (point - pivot) + pivot;
        }

        public static Vector2 RoundToDecimal(Vector2 value, int decimalPlace)
        {
            return new Vector2(
                RoundToDecimal(value.x, decimalPlace),
                RoundToDecimal(value.y, decimalPlace));
        }
        
        public static Vector2 FloorToDecimal(Vector2 value, int decimalPlace)
        {
            return new Vector2(
                FloorToDecimal(value.x, decimalPlace),
                FloorToDecimal(value.y, decimalPlace));
        }
        
        public static Vector2 RoundTowardsZeroToDecimal(Vector2 value, int decimalPlace)
        {
            return new Vector2(
                RoundTowardsZeroToDecimal(value.x, decimalPlace),
                RoundTowardsZeroToDecimal(value.y, decimalPlace));
        }

        public static float RoundToDecimal(float value, int decimalPlace)
        {
            float multiplier = Mathf.Pow(10, decimalPlace);

            value *= multiplier;
            value = Mathf.Round(value);
            value /= multiplier;

            return value;
        }

        public static float CeilToDecimal(float value, int decimalPlace)
        {
            float multiplier = Mathf.Pow(10, decimalPlace);

            value *= multiplier;
            value = Mathf.Ceil(value);
            value /= multiplier;

            return value;
        }

        public static float RoundTowardsZeroToDecimal(float value, int decimalPlace)
        {
            float multiplier = Mathf.Pow(10, decimalPlace);

            value *= multiplier;
            value = value < 0 ? Mathf.Ceil(value) : Mathf.Floor(value);
            value /= multiplier;

            return value;
        }

        public static float FloorToDecimal(float value, int decimalPlace)
        {
            float multiplier = Mathf.Pow(10, decimalPlace);

            value *= multiplier;
            value = Mathf.Floor(value);
            value /= multiplier;

            return value;
        }

        public static bool CompareType<T>(T obj1, T obj2)
        {
            return obj1.GetType() == obj2.GetType();
        }

        public static Vector2 GetLocalPosition(Vector2 worldPos, Transform parent)
        {
            return parent.InverseTransformPoint(worldPos);
        }

        public static bool TryGetKey<K, V>(this Dictionary<K, V> dictionary, V value, out K key)
        {
            if (value == null)
            {
                key = default;
                return false;
            }

            foreach (var valuePair in dictionary)
                if (ReferenceEquals(valuePair.Value, value))
                {
                    key = valuePair.Key;
                    return true;
                }

            key = default;
            return false;
        }
        
        public static Vector3 ProjectDirectionOntoNormal(Vector3 dir, Vector3 normal)
        {
            float y = (-normal.x * dir.x - normal.z * dir.z) / normal.y;
            return new Vector3(dir.x, y, dir.z).normalized;
        }

        public static float DirectionToAngle(Vector2 dir)
        {
            return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        }
        
        public static float DirectionToAngleInverseAtan(Vector2 dir)
        {
            return Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
        }

        public static Vector3 IgnoreY(this Vector3 vec, bool normalized = false)
        {
            return normalized ? new Vector3(vec.x, 0f, vec.z).normalized : new Vector3(vec.x, 0f, vec.z);
        }

        public static Vector2 To2D(this Vector3 vec, bool normalized = false)
        {
            return normalized ? new Vector2(vec.x, vec.z).normalized : new Vector2(vec.x, vec.z);
        }

        public static Vector3 ProjectOnPlaneRetainMag(Vector3 vector, Vector3 plane)
        {
            return ProjectDirectionOntoNormal(vector.normalized, plane) * vector.magnitude;
        }

        public static T[] GetSubArray<T>(this T[] arr, int startIndex)
        {
            if (arr.Length == 0)
                return arr;
            if (startIndex >= arr.Length)
                return null;
            
            T[] result = new T[arr.Length - startIndex];
            Array.Copy(arr, startIndex, result, 0, result.Length);
            return result;
        }
        
        public static float ClampAngle(float angle, float min, float max)
        {
            if (min < 0 && max > 0 && (angle > max || angle < min))
            {
                angle -= 360;
                if (angle > max || angle < min)
                {
                    return Mathf.Abs(Mathf.DeltaAngle(angle, min)) < 
                           Mathf.Abs(Mathf.DeltaAngle(angle, max)) ? min : max;
                }
            }
            else if(min > 0 && (angle > max || angle < min))
            {
                angle += 360;
                if (angle > max || angle < min)
                {
                    return Mathf.Abs(Mathf.DeltaAngle(angle, min)) < 
                           Mathf.Abs(Mathf.DeltaAngle(angle, max)) ? min : max;
                }
            }
 
            if (angle < min) return min;
            return angle > max ? max : angle;
        }

        public static float NormalizeAngle(float angle)
        {
            while (angle < 0f)
                angle += 360f;

            return angle % 360f;
        }

        public static Vector2 ClampVectorInRadius(Vector2 vector, Vector2 center, float radius)
        {
            Vector2 offset = vector - center;

            return center + Vector2.ClampMagnitude(offset, radius);
        }
        
        public static Vector2 ClampVectorToRadius(Vector2 vector, Vector2 center, float radius)
        {
            Vector2 dir = (vector - center).normalized;

            return center + dir * radius;
        }

        public static bool IsValidIndex<T>(this List<T> list, int index)
        {
            return index >= 0 && index < list.Count;
        }

        public static Vector2 ClampVectorOutsideRadius(Vector2 vector, Vector2 center, float radius)
        {
            float dist = Vector2.Distance(vector, center);
            if (dist < radius)
                return (vector - center).normalized * radius + center;
            return vector;
        }

        public static Color GetWithNewAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }
        
        public static Vector2 ClampVector(Vector2 vector, Vector2 min, Vector2 max)
        {
            return new Vector2(Mathf.Clamp(vector.x, min.x, max.x), Mathf.Clamp(vector.y, min.y, max.y));
        }

        public static Vector2 AngleToDirection(float angle)
        {
            return new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        }

        public static Vector2 GetNormalizedDirectionBetween(Vector2 targetPos, Vector2 myPos)
        {
            return (targetPos - myPos).normalized;
        }

        public static RaycastHit2D CamCast(Camera cam)
        {
            return Physics2D.Raycast(cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        }

        public static bool IsPointInRT(Vector2 point, RectTransform rt)
        {
            // Get the left, right, top, and bottom boundaries of the rect
            float leftSide = rt.position.x - rt.sizeDelta.x / 2f;
            float rightSide = rt.position.x + rt.sizeDelta.x / 2f;
            float topSide = rt.position.y + rt.sizeDelta.y / 2f;
            float bottomSide = rt.position.y - rt.sizeDelta.y / 2f;

            // Check to see if the point is in the calculated bounds
            if (point.x >= leftSide &&
                point.x <= rightSide &&
                point.y >= bottomSide &&
                point.y <= topSide)
                return true;
            return false;
        }

        public static bool IsPointInCollider(Collider col, Vector3 point)
        {
            return point.Equals(col.ClosestPoint(point));
        }

        public static string GetColoredRichText(string text, Color color)
        {
            return "<color=" + "#" + ColorUtility.ToHtmlStringRGBA(color) + ">" + text + "</color>";
        }

        public static string MakeBold(this string text)
        {
            return "<b>" + text + "</b>";
        }

        public static string MakeUnderlined(this string text)
        {
            return "<u>" + text + "</u>";
        }
    }
}