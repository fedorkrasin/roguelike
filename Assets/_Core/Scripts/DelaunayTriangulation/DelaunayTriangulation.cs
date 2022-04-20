using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Core.Scripts.DelaunayTriangulation
{
    public class DelaunayTriangulation
    {
        private List<Point> _vertices;
        private List<Edge> _edges;
        private List<Triangle> _triangles;
        private List<Tetrahedron> _tetrahedrons;

        public List<Point> Vertices => _vertices;
        public List<Edge> Edges => _edges;
        public List<Triangle> Triangles => _triangles;
        public List<Tetrahedron> Tetrahedrons => _tetrahedrons;

        public static DelaunayTriangulation Triangulate(List<Point> vertices)
        {
            var delaunay = new DelaunayTriangulation
            {
                _vertices = new List<Point>(vertices),
                _edges = new List<Edge>(),
                _triangles = new List<Triangle>(),
                _tetrahedrons = new List<Tetrahedron>()
            };

            delaunay.Triangulate();
            return delaunay;
        }

        public static bool ArePointsEqual(Point left, Point right)
        {
            return Vector3.Distance(left.Position, right.Position) < 0.1f;
        }

        private void Triangulate()
        {
            var superTetrahedron = GenerateSuperTetrahedron();
            _tetrahedrons.Add(superTetrahedron);

            foreach (var vertex in Vertices)
            {
                var triangles = new List<Triangle>();

                foreach (var t in _tetrahedrons.Where(t => t.IsInsideCircumsphere(vertex.Position)))
                {
                    t.IsBad = true;
                    triangles.Add(new Triangle(t.Vertices[0], t.Vertices[1], t.Vertices[2]));
                    triangles.Add(new Triangle(t.Vertices[0], t.Vertices[1], t.Vertices[3]));
                    triangles.Add(new Triangle(t.Vertices[0], t.Vertices[2], t.Vertices[3]));
                    triangles.Add(new Triangle(t.Vertices[1], t.Vertices[2], t.Vertices[3]));
                }

                for (var i = 0; i < triangles.Count; i++)
                {
                    for (var j = i + 1; j < triangles.Count; j++)
                    {
                        if (Triangle.AlmostEqual(triangles[i], triangles[j]))
                        {
                            triangles[i].IsBad = true;
                            triangles[j].IsBad = true;
                        }
                    }
                }

                _tetrahedrons.RemoveAll(tetrahedron => tetrahedron.IsBad);
                triangles.RemoveAll(triangle => triangle.IsBad);

                foreach (var triangle in triangles)
                {
                    _tetrahedrons.Add(new Tetrahedron(triangle.Vertices[0], triangle.Vertices[1], triangle.Vertices[2],
                        vertex));
                }
            }

            _tetrahedrons.RemoveAll(t =>
                t.ContainsVertex(superTetrahedron.Vertices[0]) || t.ContainsVertex(superTetrahedron.Vertices[1]) ||
                t.ContainsVertex(superTetrahedron.Vertices[2]) || t.ContainsVertex(superTetrahedron.Vertices[3]));

            AddEdges();
            AddTriangles();
        }

        private Tetrahedron GenerateSuperTetrahedron()
        {
            var minX = Vertices[0].Position.x;
            var minY = Vertices[0].Position.y;
            var minZ = Vertices[0].Position.z;
            var maxX = minX;
            var maxY = minY;
            var maxZ = minZ;

            foreach (var vertex in Vertices)
            {
                if (vertex.Position.x < minX) minX = vertex.Position.x;
                if (vertex.Position.x > maxX) maxX = vertex.Position.x;
                if (vertex.Position.y < minY) minY = vertex.Position.y;
                if (vertex.Position.y > maxY) maxY = vertex.Position.y;
                if (vertex.Position.z < minZ) minZ = vertex.Position.z;
                if (vertex.Position.z > maxZ) maxZ = vertex.Position.z;
            }

            var dx = maxX - minX;
            var dy = maxY - minY;
            var dz = maxZ - minZ;
            var deltaMax = Mathf.Max(dx, dy, dz) * 2;

            var point1 = new Point(new Vector3(minX - 1, minY - 1, minZ - 1));
            var point2 = new Point(new Vector3(maxX + deltaMax, minY - 1, minZ - 1));
            var point3 = new Point(new Vector3(minX - 1, maxY + deltaMax, minZ - 1));
            var point4 = new Point(new Vector3(minX - 1, minY - 1, maxZ + deltaMax));

            return new Tetrahedron(point1, point2, point3, point4);
        }

        private void AddEdges()
        {
            var edgeSet = new HashSet<Edge>();

            foreach (var t in _tetrahedrons)
            {
                var ab = new Edge(t.Vertices[0], t.Vertices[1]);
                var bc = new Edge(t.Vertices[1], t.Vertices[2]);
                var ca = new Edge(t.Vertices[2], t.Vertices[0]);
                var da = new Edge(t.Vertices[3], t.Vertices[0]);
                var db = new Edge(t.Vertices[3], t.Vertices[1]);
                var dc = new Edge(t.Vertices[3], t.Vertices[2]);

                if (edgeSet.Add(ab)) _edges.Add(ab);
                if (edgeSet.Add(bc)) _edges.Add(bc);
                if (edgeSet.Add(ca)) _edges.Add(ca);
                if (edgeSet.Add(da)) _edges.Add(da);
                if (edgeSet.Add(db)) _edges.Add(db);
                if (edgeSet.Add(dc)) _edges.Add(dc);
            }
        }

        private void AddTriangles()
        {
            var triangleSet = new HashSet<Triangle>();

            foreach (var t in _tetrahedrons)
            {
                var abc = new Triangle(t.Vertices[0], t.Vertices[1], t.Vertices[2]);
                var abd = new Triangle(t.Vertices[0], t.Vertices[1], t.Vertices[3]);
                var acd = new Triangle(t.Vertices[0], t.Vertices[2], t.Vertices[3]);
                var bcd = new Triangle(t.Vertices[1], t.Vertices[2], t.Vertices[3]);

                if (triangleSet.Add(abc)) _triangles.Add(abc);
                if (triangleSet.Add(abd)) _triangles.Add(abd);
                if (triangleSet.Add(acd)) _triangles.Add(acd);
                if (triangleSet.Add(bcd)) _triangles.Add(bcd);
            }
        }
    }
}
