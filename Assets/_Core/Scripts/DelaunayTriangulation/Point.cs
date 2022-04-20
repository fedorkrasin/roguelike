using UnityEngine;

namespace _Core.Scripts.DelaunayTriangulation
{
    public class Point
    {
        public float X { get; }
        public float Y { get; }
        public float Z { get; }

        public Vector3 Position => new(X, Y, Z);

        public Point(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Point(Vector3 vector)
        {
            X = vector.x;
            Y = vector.y;
            Z = vector.z;
        }

        public static Point operator +(Point a, Point b)
        {
            return new Point(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Point operator -(Point a, Point b)
        {
            return new Point(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public override string ToString()
        {
            return $"(x: {X}, y: {Y}, z: {Z})";
        }
    }
}

