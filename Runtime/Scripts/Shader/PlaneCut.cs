using System.Collections.Generic;
using UnityEngine;

namespace NgoUyenNguyen.Shader
{
    public class PlaneCut : MonoBehaviour
    {
        [Header("Cut Renderer")]
        [SerializeField] private Renderer _objRend;
        [Header("Moving Point on Mesh")]
        [SerializeField] private MeshFilter _objMeshFilter;
        [SerializeField] private Transform _point;




        public Renderer objRend { get => _objRend; set => _objRend = value; }
        public MeshFilter objMeshFilter { get => _objMeshFilter; set => _objMeshFilter = value; }
        public Transform point { get => _point; set => _point = value; }






        // Update is called once per frame
        void Update()
        {
            // Set Alpha Clipping
            _objRend.material.SetVector("_PlaneOrigin", transform.position - transform.up);
            _objRend.material.SetVector("_PlaneNormal", transform.up);

            // Move point to the next point in a clockwise direction
            if (_point != null)
            {
                _point.position = GetNextPointClockwise(GetIntersectionPoints(_objMeshFilter, new Plane(transform.up, transform.position)), _point.position);
            }
        }





        // Method to get the next point in a clockwise direction from the current position
        private Vector3 GetNextPointClockwise(List<Vector3> points, Vector3 currentPos)
        {
            if (points.Count == 0) return currentPos;

            // Calculate the center of the points
            Vector3 center = Vector3.zero;
            foreach (var p in points) center += p;
            center /= points.Count;

            // Calculate current angle
            Vector2 currentDir = new Vector2(currentPos.x - center.x, currentPos.z - center.z);
            float currentAngleDeg = Mathf.Atan2(currentDir.y, currentDir.x) * Mathf.Rad2Deg;

            Vector3 nextPoint = currentPos;
            float smallestPositiveDiff = 360f;

            // FInd the next point in a clockwise direction
            foreach (var p in points)
            {
                Vector2 dir = new Vector2(p.x - center.x, p.z - center.z);
                float angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                float diff = Mathf.DeltaAngle(currentAngleDeg, angleDeg);

                if (diff > 0 && diff < smallestPositiveDiff)
                {
                    smallestPositiveDiff = diff;
                    nextPoint = p;
                }
            }

            // If no positive difference was found, return the closest point
            if (Mathf.Approximately(smallestPositiveDiff, 360f))
            {
                float minDist = float.MaxValue;
                foreach (var p in points)
                {
                    float dist = Vector3.Distance(currentPos, p);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nextPoint = p;
                    }
                }
            }

            return nextPoint;
        }





        // Method to get intersection points of a mesh with a plane
        private List<Vector3> GetIntersectionPoints(MeshFilter meshFilter, Plane plane)
        {
            Mesh mesh = meshFilter.mesh;
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            List<Vector3> intersectionPoints = new List<Vector3>();

            for (int i = 0; i < triangles.Length; i += 3)
            {
                int i0 = triangles[i];
                int i1 = triangles[i + 1];
                int i2 = triangles[i + 2];

                Vector3 p0 = meshFilter.transform.TransformPoint(vertices[i0]);
                Vector3 p1 = meshFilter.transform.TransformPoint(vertices[i1]);
                Vector3 p2 = meshFilter.transform.TransformPoint(vertices[i2]);

                CheckEdge(p0, p1, plane, intersectionPoints);
                CheckEdge(p1, p2, plane, intersectionPoints);
                CheckEdge(p2, p0, plane, intersectionPoints);
            }
            return intersectionPoints;
        }

        private void CheckEdge(Vector3 a, Vector3 b, Plane plane, List<Vector3> points)
        {
            bool aSide = plane.GetSide(a);
            bool bSide = plane.GetSide(b);

            if (aSide != bSide)
            {
                float distA = plane.GetDistanceToPoint(a);
                float distB = plane.GetDistanceToPoint(b);
                float t = distA / (distA - distB);
                Vector3 point = Vector3.Lerp(a, b, t);
                points.Add(point);
            }
        }

    }
}
