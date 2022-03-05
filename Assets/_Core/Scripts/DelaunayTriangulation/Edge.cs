using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge
{
    public Point Point1 { get; }
    public Point Point2 { get; }

    public Edge(Point point1, Point point2)
    {
        Point1 = point1;
        Point2 = point2;
    }
    
    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        if (obj.GetType() != GetType()) return false;
        var edge = obj as Edge;

        var samePoints = Point1 == edge.Point1 && Point2 == edge.Point2;
        var samePointsReversed = Point1 == edge.Point2 && Point2 == edge.Point1;
        return samePoints || samePointsReversed;
    }
}
