using UnityEngine;

namespace NgoUyenNguyen
{
    public static class MathExtension
    {
        public static float Remap(float value, float fromIn, float toIn, float fromOut, float toOut) 
            => Mathf.Lerp(fromOut, toOut, Mathf.InverseLerp(fromIn, toIn, value));

        public static bool Intersect(Vector3 p1, Vector3 n1, Vector3 p2, Vector3 n2, out Vector3 intersection)
        {
            intersection = Vector3.zero;

            var n1xn2 = Vector3.Cross(n1, n2);

            var crossMagSq = n1xn2.sqrMagnitude;
            if (crossMagSq < Mathf.Epsilon)
                return false;

            var diff = p2 - p1;

            var coplanarTest = Vector3.Dot(n1xn2, diff);
            if (Mathf.Abs(coplanarTest) > 1e-5f)
                return false;

            var t = Vector3.Dot(Vector3.Cross(diff, n2), n1xn2) / crossMagSq;

            intersection = p1 + n1 * t;
            return true;
        }
    }
}
