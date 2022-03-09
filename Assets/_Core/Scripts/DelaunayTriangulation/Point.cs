using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Point
{
    public float X { get; }
    public float Y { get; }
    public float Z { get; }

    public HashSet<Triangle> AdjacentTriangles { get; } = new HashSet<Triangle>();

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
    
    public Vector3 ToVector3() => new Vector3(X, Y, Z);

    public static Point operator +(Point a, Point b)
    {
        return new Point(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }

    public static Point operator -(Point a, Point b)
    {
        return new Point(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }

    public override string ToString() => $"(x: {X}, y: {Y}, z: {Z})";
}

