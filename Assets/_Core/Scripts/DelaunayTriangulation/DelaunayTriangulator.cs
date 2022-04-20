using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DelaunayTriangulator
{
    // private static void GenerateBorder()
    private static readonly List<Triangle> border = new List<Triangle>();
    
    public static List<Triangle> BowyerWatson(IEnumerable<Point> points)
    {
        var p1 = new Point(-500, -500, -500);
        var p2 = new Point(-500, 0, 500);
        var p3 = new Point(500, 500, 500);
        var borderTriangle = new Triangle(p1, p2, p3);
        border.Add(borderTriangle);
        
        var triangulation = new HashSet<Triangle>(border);

        foreach (var point in points)
        {
            var badTriangles = FindBadTriangles(point, triangulation);
            var polygon = FindHoleBoundaries(badTriangles);

            Debug.Log($"bad: {badTriangles.Count}");
            Debug.Log($"poly: {polygon.Count}");

            foreach (var triangle in badTriangles)
            {
                foreach (var vertex in triangle.Vertices)
                {
                    vertex.AdjacentTriangles.Remove(triangle);
                }
            }
            
            triangulation.RemoveWhere(o => badTriangles.Contains(o));

            foreach (var edge in polygon.Where(possibleEdge => possibleEdge.Point1 != point && possibleEdge.Point2 != point))
            {
                var triangle = new Triangle(point, edge.Point1, edge.Point2);
                triangulation.Add(triangle);
            }
        }
        
        return triangulation.ToList();
    }
    
    private static List<Edge> FindHoleBoundaries(ISet<Triangle> badTriangles)
    {
        var edges = new List<Edge>();
        
        foreach (var triangle in badTriangles)
        {
            edges.Add(new Edge(triangle.Vertices[0], triangle.Vertices[1]));
            edges.Add(new Edge(triangle.Vertices[1], triangle.Vertices[2]));
            edges.Add(new Edge(triangle.Vertices[2], triangle.Vertices[0]));
        }
        
        var boundaryEdges = edges.GroupBy(o => o).Where(o => o.Count() == 1).Select(o => o.First());
        return boundaryEdges.ToList();
    }

    private static ISet<Triangle> FindBadTriangles(Point point, HashSet<Triangle> triangles)
    {
        var badTriangles = triangles.Where(o => o.IsPointInsideCircumsphere(point));
        return new HashSet<Triangle>(badTriangles);
    }
}
