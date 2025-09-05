using System.Collections.Generic;
using UnityEngine;

namespace NgoUyenNguyen.Line
{
    [RequireComponent(typeof(LineRenderer))]
    public class RoundedCornerLine : MonoBehaviour
    {
        [Header("Input Points")]
        public Vector3[] controlPoints = new Vector3[0];

        [Header("Corner Settings")]
        [Range(0, 1)] public float cornerRadius = 0.5f;
        public int cornerSegments = 6;

        private LineRenderer line;







        void Awake()
        {
            line = GetComponent<LineRenderer>();
        }

        private void Update()
        {
            UpdateLine();
        }

        public void UpdateLine()
        {
            Vector3[] rounded = AddRoundedCorners(controlPoints, cornerRadius, cornerSegments, line.loop);

            line.positionCount = rounded.Length;
            line.SetPositions(rounded);
        }

        public Vector3[] AddRoundedCorners(Vector3[] pts, float radius, int segments, bool loop = false)
        {
            // If no corners, return
            if (pts.Length < 3) return pts;

            List<Vector3> result = new List<Vector3>();

            if (!loop) result.Add(pts[0]);

            for (int i = loop ? 0 : 1, max = loop ? pts.Length : pts.Length - 1; i < max; i++)
            {
                Vector3 prev = loop ? pts[(i - 1 + pts.Length) % pts.Length] : pts[i - 1];
                Vector3 current = pts[i];
                Vector3 next = loop ? pts[(i + 1) % pts.Length] : pts[i + 1];

                Vector3 dir1 = (prev - current).normalized;
                Vector3 dir2 = (next - current).normalized;

                float angle = Vector3.Angle(dir1, dir2) * Mathf.Deg2Rad;
                float halfAngle = angle / 2f;

                float cutDist = radius / Mathf.Tan(halfAngle);

                float len1 = (prev - current).magnitude;
                float len2 = (next - current).magnitude;

                float dist1 = Mathf.Min(cutDist, len1 - 0.001f);
                float dist2 = Mathf.Min(cutDist, len2 - 0.001f);

                Vector3 p1 = current + dir1 * dist1;
                Vector3 p2 = current + dir2 * dist2;

                Vector3 bisector = (dir1 + dir2).normalized;
                float offset = radius / Mathf.Sin(halfAngle);
                Vector3 center = current + bisector * offset;

                Vector3 startDir = (p1 - center).normalized;
                Vector3 endDir = (p2 - center).normalized;

                Vector3 normal = Vector3.Cross(dir1, dir2).normalized;

                float sweepAngle = Vector3.SignedAngle(startDir, endDir, normal);

                result.Add(p1);

                for (int j = 1; j < segments; j++)
                {
                    float t = j / (float)segments;
                    Quaternion rot = Quaternion.AngleAxis(sweepAngle * t, normal);
                    Vector3 arcPoint = center + rot * startDir * radius;
                    result.Add(arcPoint);
                }

                result.Add(p2);
            }

            if (!loop) result.Add(pts[pts.Length - 1]);
            return result.ToArray();
        }
    }
}
