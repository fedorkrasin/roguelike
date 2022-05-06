using System;
using System.Collections.Generic;
using _Core.Scripts.PriorityQueue;
using UnityEngine;

namespace _Core.Scripts.DungeonPathfinder
{
    public class Pathfinder
    {
        private static readonly Vector3Int[] Neighbors =
        {
            new(1, 0, 0),
            new(-1, 0, 0),
            new(0, 0, 1),
            new(0, 0, -1),

            new(3, 1, 0),
            new(-3, 1, 0),
            new(0, 1, 3),
            new(0, 1, -3),

            new(3, -1, 0),
            new(-3, -1, 0),
            new(0, -1, 3),
            new(0, -1, -3),
        };

        private Grid<Node> _grid;
        private SimplePriorityQueue<Node, float> _queue;
        private HashSet<Node> _closed;
        private Stack<Vector3Int> _stack;

        public Pathfinder(Vector3Int size)
        {
            _grid = new Grid<Node>(size, Vector3Int.zero);
            _queue = new SimplePriorityQueue<Node, float>();
            _closed = new HashSet<Node>();
            _stack = new Stack<Vector3Int>();

            for (var x = 0; x < size.x; x++)
            for (var y = 0; y < size.y; y++)
            for (var z = 0; z < size.z; z++)
            {
                _grid[x, y, z] = new Node(new Vector3Int(x, y, z));
            }
        }

        private void ResetNodes()
        {
            var size = _grid.Size;

            for (var x = 0; x < size.x; x++)
            for (var y = 0; y < size.y; y++)
            for (var z = 0; z < size.z; z++)
            {
                var node = _grid[x, y, z];
                node.Previous = null;
                node.Cost = float.PositiveInfinity;
                node.PreviousSet.Clear();
            }
        }

        public List<Vector3Int> FindPath(Vector3Int start, Vector3Int end, Func<Node, Node, PathCost> costFunction)
        {
            ResetNodes();
            _queue.Clear();
            _closed.Clear();

            _queue = new SimplePriorityQueue<Node, float>();
            _closed = new HashSet<Node>();

            _grid[start].Cost = 0;
            _queue.Enqueue(_grid[start], 0);

            while (_queue.Count > 0)
            {
                var node = _queue.Dequeue();
                _closed.Add(node);

                if (node.Position == end)
                {
                    return ReconstructPath(node);
                }

                foreach (var offset in Neighbors)
                {
                    if (!_grid.InBounds(node.Position + offset)) continue;
                    var neighbor = _grid[node.Position + offset];
                    if (_closed.Contains(neighbor)) continue;

                    if (node.PreviousSet.Contains(neighbor.Position))
                    {
                        continue;
                    }

                    var pathCost = costFunction(node, neighbor);
                    if (!pathCost.traversable) continue;

                    if (pathCost.isStairs)
                    {
                        var xDir = Mathf.Clamp(offset.x, -1, 1);
                        var zDir = Mathf.Clamp(offset.z, -1, 1);
                        var verticalOffset = new Vector3Int(0, offset.y, 0);
                        var horizontalOffset = new Vector3Int(xDir, 0, zDir);

                        if (node.PreviousSet.Contains(node.Position + horizontalOffset)
                            || node.PreviousSet.Contains(node.Position + horizontalOffset * 2)
                            || node.PreviousSet.Contains(node.Position + verticalOffset + horizontalOffset)
                            || node.PreviousSet.Contains(node.Position + verticalOffset + horizontalOffset * 2))
                        {
                            continue;
                        }
                    }

                    var newCost = node.Cost + pathCost.cost;

                    if (newCost < neighbor.Cost)
                    {
                        neighbor.Previous = node;
                        neighbor.Cost = newCost;

                        if (_queue.TryGetPriority(node, out var existingPriority))
                        {
                            _queue.UpdatePriority(node, newCost);
                        }
                        else
                        {
                            _queue.Enqueue(neighbor, neighbor.Cost);
                        }

                        neighbor.PreviousSet.Clear();
                        neighbor.PreviousSet.UnionWith(node.PreviousSet);
                        neighbor.PreviousSet.Add(node.Position);

                        if (pathCost.isStairs)
                        {
                            var xDir = Mathf.Clamp(offset.x, -1, 1);
                            var zDir = Mathf.Clamp(offset.z, -1, 1);
                            var verticalOffset = new Vector3Int(0, offset.y, 0);
                            var horizontalOffset = new Vector3Int(xDir, 0, zDir);

                            neighbor.PreviousSet.Add(node.Position + horizontalOffset);
                            neighbor.PreviousSet.Add(node.Position + horizontalOffset * 2);
                            neighbor.PreviousSet.Add(node.Position + verticalOffset + horizontalOffset);
                            neighbor.PreviousSet.Add(node.Position + verticalOffset + horizontalOffset * 2);
                        }
                    }
                }
            }

            return null;
        }

        private List<Vector3Int> ReconstructPath(Node node)
        {
            var result = new List<Vector3Int>();

            while (node != null)
            {
                _stack.Push(node.Position);
                node = node.Previous;
            }

            while (_stack.Count > 0)
            {
                result.Add(_stack.Pop());
            }

            return result;
        }
    }
}