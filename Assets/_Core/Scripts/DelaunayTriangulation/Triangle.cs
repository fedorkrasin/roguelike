namespace _Core.Scripts.DelaunayTriangulation
{
    public class Triangle
    {
        public Point[] Vertices { get; } = new Point[3];

        public bool IsBad { get; set; }

        public Triangle(Point point1, Point point2, Point point3)
        {
            Vertices[0] = point1;
            Vertices[1] = point3;
            Vertices[2] = point2;
        }

        public static bool operator ==(Triangle left, Triangle right)
        {
            return (left.Vertices[0] == right.Vertices[0] || left.Vertices[0] == right.Vertices[1] ||
                    left.Vertices[0] == right.Vertices[2])
                   && (left.Vertices[1] == right.Vertices[0] || left.Vertices[1] == right.Vertices[1] ||
                       left.Vertices[1] == right.Vertices[2])
                   && (left.Vertices[2] == right.Vertices[0] || left.Vertices[2] == right.Vertices[1] ||
                       left.Vertices[2] == right.Vertices[2]);
        }

        public static bool operator !=(Triangle left, Triangle right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Triangle e)
            {
                return this == e;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Vertices[0].GetHashCode() ^ Vertices[1].GetHashCode() ^ Vertices[2].GetHashCode();
        }

        public static bool AlmostEqual(Triangle left, Triangle right)
        {
            return (DelaunayTriangulation.ArePointsEqual(left.Vertices[0], right.Vertices[0]) ||
                    DelaunayTriangulation.ArePointsEqual(left.Vertices[0], right.Vertices[1]) ||
                    DelaunayTriangulation.ArePointsEqual(left.Vertices[0], right.Vertices[2]))
                   && (DelaunayTriangulation.ArePointsEqual(left.Vertices[1], right.Vertices[0]) ||
                       DelaunayTriangulation.ArePointsEqual(left.Vertices[1], right.Vertices[1]) ||
                       DelaunayTriangulation.ArePointsEqual(left.Vertices[1], right.Vertices[2]))
                   && (DelaunayTriangulation.ArePointsEqual(left.Vertices[2], right.Vertices[0]) ||
                       DelaunayTriangulation.ArePointsEqual(left.Vertices[2], right.Vertices[1]) ||
                       DelaunayTriangulation.ArePointsEqual(left.Vertices[2], right.Vertices[2]));
        }

        public override string ToString()
        {
            return $"Triangle: {Vertices[0]}; {Vertices[1]}; {Vertices[2]}";
        }
    }
}