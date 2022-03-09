using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Triangle
{
    public Point[] Vertices { get; } = new Point[3];
    public Point Circumcenter { get; private set; }
    public float CircumsphereRadius;

    public Triangle(Point point1, Point point2, Point point3)
    {
        if (point1 == point2 || point1 == point3 || point2 == point3)
        {
            throw new ArgumentException("Must be 3 distinct points");
        }

        if (!IsCounterClockwise(point1, point2, point3))
        {
            Vertices[0] = point1;
            Vertices[1] = point3;
            Vertices[2] = point2;
        }
        else
        {
            Vertices[0] = point1;
            Vertices[1] = point2;
            Vertices[2] = point3;
        }

        Vertices[0].AdjacentTriangles.Add(this);
        Vertices[1].AdjacentTriangles.Add(this);
        Vertices[2].AdjacentTriangles.Add(this);
        UpdateCircumsphere();
    }

    public IEnumerable<Triangle> TrianglesWithSharedEdge
    {
        get
        {
            var neighbors = new HashSet<Triangle>();
            foreach (var vertex in Vertices)
            {
                var trianglesWithSharedEdge = vertex.AdjacentTriangles.Where(o => o != this && SharesEdgeWith(o));
                neighbors.UnionWith(trianglesWithSharedEdge);
            }

            return neighbors;
        }
    }

    private void UpdateCircumsphere()
    {
        var a = Vertices[0];
        var b = Vertices[1];
        var c = Vertices[2];

        var ac = c - a;
        var ab = b - a;
        var abXac = Vector3.Cross(ab.ToVector3(), ac.ToVector3());

        var toCircumsphereCenter =
            (Vector3.Cross(abXac, ab.ToVector3()) * ac.ToVector3().sqrMagnitude +
             Vector3.Cross(ac.ToVector3(), abXac) * ab.ToVector3().sqrMagnitude) / (2f * abXac.sqrMagnitude);

        Circumcenter = a + new Point(toCircumsphereCenter);
        CircumsphereRadius = toCircumsphereCenter.magnitude;
    }

    private bool IsCounterClockwise(Point point1, Point point2, Point point3)
    {
        var result = (point2.X - point1.X) * (point3.Y - point1.Y) -
                     (point3.X - point1.X) * (point2.Y - point1.Y);
        return result > 0;
    }

    private bool SharesEdgeWith(Triangle triangle)
    {
        var sharedVertices = Vertices.Count(o => triangle.Vertices.Contains(o));
        return sharedVertices == 2;
    }

    public bool IsPointInsideCircumsphere(Point point)
    {
        return Vector3.Distance(Circumcenter.ToVector3(), point.ToVector3()) < CircumsphereRadius;
    }

    public override string ToString() => $"Triangle: {Vertices[0]}, {Vertices[1]}, {Vertices[2]}";
}
