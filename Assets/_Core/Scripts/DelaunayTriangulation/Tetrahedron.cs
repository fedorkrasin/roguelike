using System;
using UnityEngine;

namespace _Core.Scripts.DelaunayTriangulation
{
    public class Tetrahedron : IEquatable<Tetrahedron>
    {
        private Vector3 _circumcenter;
        private float _circumradius;
        
        public Point[] Vertices { get; } = new Point[4];

        public bool IsBad { get; set; }

        public Tetrahedron(Point point1, Point point2, Point point3, Point point4)
        {
            Vertices[0] = point1;
            Vertices[1] = point2;
            Vertices[2] = point3;
            Vertices[3] = point4;
            CalculateCircumsphere();
        }

        private void CalculateCircumsphere()
        {
            var a = new Matrix4x4(
                new Vector4(Vertices[0].Position.x, Vertices[1].Position.x, Vertices[2].Position.x, Vertices[3].Position.x),
                new Vector4(Vertices[0].Position.y, Vertices[1].Position.y, Vertices[2].Position.y, Vertices[3].Position.y),
                new Vector4(Vertices[0].Position.z, Vertices[1].Position.z, Vertices[2].Position.z, Vertices[3].Position.z),
                new Vector4(1, 1, 1, 1)
            ).determinant;

            var vertex0PositionSquared = Vertices[0].Position.sqrMagnitude;
            var vertex1PositionSquared = Vertices[1].Position.sqrMagnitude;
            var vertex2PositionSquared = Vertices[2].Position.sqrMagnitude;
            var vertex3PositionSquared = Vertices[3].Position.sqrMagnitude;

            var Dx = new Matrix4x4(
                new Vector4(vertex0PositionSquared, vertex1PositionSquared, vertex2PositionSquared, vertex3PositionSquared),
                new Vector4(Vertices[0].Position.y, Vertices[1].Position.y, Vertices[2].Position.y, Vertices[3].Position.y),
                new Vector4(Vertices[0].Position.z, Vertices[1].Position.z, Vertices[2].Position.z, Vertices[3].Position.z),
                new Vector4(1, 1, 1, 1)
            ).determinant;

            var Dy = -new Matrix4x4(
                new Vector4(vertex0PositionSquared, vertex1PositionSquared, vertex2PositionSquared, vertex3PositionSquared),
                new Vector4(Vertices[0].Position.x, Vertices[1].Position.x, Vertices[2].Position.x, Vertices[3].Position.x),
                new Vector4(Vertices[0].Position.z, Vertices[1].Position.z, Vertices[2].Position.z, Vertices[3].Position.z),
                new Vector4(1, 1, 1, 1)
            ).determinant;

            var Dz = new Matrix4x4(
                new Vector4(vertex0PositionSquared, vertex1PositionSquared, vertex2PositionSquared, vertex3PositionSquared),
                new Vector4(Vertices[0].Position.x, Vertices[1].Position.x, Vertices[2].Position.x, Vertices[3].Position.x),
                new Vector4(Vertices[0].Position.y, Vertices[1].Position.y, Vertices[2].Position.y, Vertices[3].Position.y),
                new Vector4(1, 1, 1, 1)
            ).determinant;

            var c = new Matrix4x4(
                new Vector4(vertex0PositionSquared, vertex1PositionSquared, vertex2PositionSquared, vertex3PositionSquared),
                new Vector4(Vertices[0].Position.x, Vertices[1].Position.x, Vertices[2].Position.x, Vertices[3].Position.x),
                new Vector4(Vertices[0].Position.y, Vertices[1].Position.y, Vertices[2].Position.y, Vertices[3].Position.y),
                new Vector4(Vertices[0].Position.z, Vertices[1].Position.z, Vertices[2].Position.z, Vertices[3].Position.z)
            ).determinant;

            _circumcenter = new Vector3(
                Dx / (2 * a),
                Dy / (2 * a),
                Dz / (2 * a)
            );

            var circumradiusSquared = ((Dx * Dx) + (Dy * Dy) + (Dz * Dz) - (4 * a * c)) / (4 * a * a);
            _circumradius = Mathf.Sqrt(circumradiusSquared);
        }

        public bool ContainsVertex(Point point)
        {
            return DelaunayTriangulation.ArePointsEqual(point, Vertices[0])
                   || DelaunayTriangulation.ArePointsEqual(point, Vertices[1])
                   || DelaunayTriangulation.ArePointsEqual(point, Vertices[2])
                   || DelaunayTriangulation.ArePointsEqual(point, Vertices[3]);
        }

        public bool IsInsideCircumsphere(Vector3 point)
        {
            var distance = Vector3.Distance(point, _circumcenter);
            return distance <= _circumradius;
        }

        public static bool operator ==(Tetrahedron left, Tetrahedron right)
        {
            return (left.Vertices[0] == right.Vertices[0] || left.Vertices[0] == right.Vertices[1] || left.Vertices[0] == right.Vertices[2] || left.Vertices[0] == right.Vertices[3])
                   && (left.Vertices[1] == right.Vertices[0] || left.Vertices[1] == right.Vertices[1] || left.Vertices[1] == right.Vertices[2] || left.Vertices[1] == right.Vertices[3])
                   && (left.Vertices[2] == right.Vertices[0] || left.Vertices[2] == right.Vertices[1] || left.Vertices[2] == right.Vertices[2] || left.Vertices[2] == right.Vertices[3])
                   && (left.Vertices[3] == right.Vertices[0] || left.Vertices[3] == right.Vertices[1] || left.Vertices[3] == right.Vertices[2] || left.Vertices[3] == right.Vertices[3]);
        }

        public static bool operator !=(Tetrahedron left, Tetrahedron right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Tetrahedron t)
            {
                return this == t;
            }

            return false;
        }

        public bool Equals(Tetrahedron t)
        {
            return this == t;
        }

        public override int GetHashCode()
        {
            return Vertices[0].GetHashCode() ^ Vertices[1].GetHashCode() ^ Vertices[2].GetHashCode() ^ Vertices[3].GetHashCode();
        }
    }
}