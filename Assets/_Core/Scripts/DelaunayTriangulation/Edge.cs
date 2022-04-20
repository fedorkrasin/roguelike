namespace _Core.Scripts.DelaunayTriangulation
{
    public class Edge
    {
        public Point Point1 { get; set; }
        public Point Point2 { get; set; }

        public Edge(Point point1, Point point2)
        {
            Point1 = point1;
            Point2 = point2;
        }

        public static bool operator ==(Edge left, Edge right)
        {
            return (left.Point1 == right.Point1 || left.Point1 == right.Point2)
                   && (left.Point2 == right.Point1 || left.Point2 == right.Point2);
        }

        public static bool operator !=(Edge left, Edge right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Edge e)
            {
                return this == e;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Point1.GetHashCode() ^ Point2.GetHashCode();
        }

        public static bool AlmostEqual(Edge left, Edge right)
        {
            return (DelaunayTriangulation.ArePointsEqual(left.Point1, right.Point1) ||
                    DelaunayTriangulation.ArePointsEqual(left.Point2, right.Point1))
                   && (DelaunayTriangulation.ArePointsEqual(left.Point1, right.Point2) ||
                       DelaunayTriangulation.ArePointsEqual(left.Point2, right.Point1));
        }
    }
}
