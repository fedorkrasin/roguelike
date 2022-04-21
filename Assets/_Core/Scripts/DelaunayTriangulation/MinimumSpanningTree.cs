using System.Collections.Generic;

namespace _Core.Scripts.DelaunayTriangulation
{
    public class MinimumSpanningTree
    {
        private readonly List<Edge> _mstEdges = new();

        private int[,] _graph;
        private float[,] _graphEdgesWeight;
        private Point[] _graphPoints;

        public static List<Edge> Build(DelaunayTriangulation triangulation)
        {
            var mst = new MinimumSpanningTree();
            return mst.GetMinimumSpanningTree(triangulation);
        }

        private List<Edge> GetMinimumSpanningTree(DelaunayTriangulation triangulation)
        {
            SetGraph(triangulation);
            BuildMinimumSpanningTree();
            return _mstEdges;
        }

        private void SetGraph(DelaunayTriangulation triangulation)
        {
            _graph = new int[triangulation.Vertices.Count, triangulation.Vertices.Count];
            _graphEdgesWeight = new float[triangulation.Vertices.Count, triangulation.Vertices.Count];
            _graphPoints = new Point[triangulation.Vertices.Count];

            foreach (var edge in triangulation.Edges)
            {
                var index1 = triangulation.Vertices.FindIndex(point => point == edge.Point1);
                var index2 = triangulation.Vertices.FindIndex(point => point == edge.Point2);

                _graph[index1, index2] = _graph[index2, index1] = 1;
                _graphEdgesWeight[index1, index2] = _graphEdgesWeight[index2, index1] = edge.Lenght;

                _graphPoints[index1] ??= edge.Point1;
                _graphPoints[index2] ??= edge.Point2;
            }
        }

        private void BuildMinimumSpanningTree()
        {
            var builtVertices = new List<int> {0};

            while (builtVertices.Count < _graph.GetLength(0))
            {
                var minWeight = float.MaxValue;
                var prevVertexIndex = 0;
                var nextVertexIndex = 0;
                
                foreach (var vertex in builtVertices)
                {
                    for (var i = 0; i < _graph.GetLength(1); i++)
                    {
                        if (builtVertices.Contains(i)) continue;
                        if (_graph[vertex, i] == 0) continue;
                        
                        if (minWeight > _graphEdgesWeight[vertex, i])
                        {
                            minWeight = _graphEdgesWeight[vertex, i];
                            prevVertexIndex = vertex;
                            nextVertexIndex = i;
                        }
                    }
                }

                var edge = new Edge(_graphPoints[prevVertexIndex], _graphPoints[nextVertexIndex]);
                _mstEdges.Add(edge);
                builtVertices.Add(nextVertexIndex);
            }
        }
    }
}