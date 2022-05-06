using System;
using System.Collections.Generic;
using _Core.Scripts.PriorityQueue;
using UnityEngine;

namespace _Core.Scripts.DungeonPathfinder
{
    public class Pathfinder
    {
        static readonly Vector3Int[] neighbors =
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

        Grid<Node> grid;
        SimplePriorityQueue<Node, float> queue;
        HashSet<Node> closed;
        Stack<Vector3Int> stack;

        public Pathfinder(Vector3Int size)
        {
            grid = new Grid<Node>(size, Vector3Int.zero);

            queue = new SimplePriorityQueue<Node, float>();
            closed = new HashSet<Node>();
            stack = new Stack<Vector3Int>();

            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    for (int z = 0; z < size.z; z++)
                    {
                        grid[x, y, z] = new Node(new Vector3Int(x, y, z));
                    }
                }
            }
        }

        private void ResetNodes()
        {
            var size = grid.Size;

            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    for (int z = 0; z < size.z; z++)
                    {
                        var node = grid[x, y, z];
                        node.Previous = null;
                        node.Cost = float.PositiveInfinity;
                        node.PreviousSet.Clear();
                    }
                }
            }
        }

        public List<Vector3Int> FindPath(Vector3Int start, Vector3Int end, Func<Node, Node, PathCost> costFunction)
        {
            ResetNodes();
            queue.Clear();
            closed.Clear();

            queue = new SimplePriorityQueue<Node, float>();
            closed = new HashSet<Node>();

            grid[start].Cost = 0;
            queue.Enqueue(grid[start], 0);

            while (queue.Count > 0)
            {
                Node node = queue.Dequeue();
                closed.Add(node);

                if (node.Position == end)
                {
                    return ReconstructPath(node);
                }

                foreach (var offset in neighbors)
                {
                    if (!grid.InBounds(node.Position + offset)) continue;
                    var neighbor = grid[node.Position + offset];
                    if (closed.Contains(neighbor)) continue;

                    if (node.PreviousSet.Contains(neighbor.Position))
                    {
                        continue;
                    }

                    var pathCost = costFunction(node, neighbor);
                    if (!pathCost.traversable) continue;

                    if (pathCost.isStairs)
                    {
                        int xDir = Mathf.Clamp(offset.x, -1, 1);
                        int zDir = Mathf.Clamp(offset.z, -1, 1);
                        Vector3Int verticalOffset = new Vector3Int(0, offset.y, 0);
                        Vector3Int horizontalOffset = new Vector3Int(xDir, 0, zDir);

                        if (node.PreviousSet.Contains(node.Position + horizontalOffset)
                            || node.PreviousSet.Contains(node.Position + horizontalOffset * 2)
                            || node.PreviousSet.Contains(node.Position + verticalOffset + horizontalOffset)
                            || node.PreviousSet.Contains(node.Position + verticalOffset + horizontalOffset * 2))
                        {
                            continue;
                        }
                    }

                    float newCost = node.Cost + pathCost.cost;

                    if (newCost < neighbor.Cost)
                    {
                        neighbor.Previous = node;
                        neighbor.Cost = newCost;

                        if (queue.TryGetPriority(node, out float existingPriority))
                        {
                            queue.UpdatePriority(node, newCost);
                        }
                        else
                        {
                            queue.Enqueue(neighbor, neighbor.Cost);
                        }

                        neighbor.PreviousSet.Clear();
                        neighbor.PreviousSet.UnionWith(node.PreviousSet);
                        neighbor.PreviousSet.Add(node.Position);

                        if (pathCost.isStairs)
                        {
                            int xDir = Mathf.Clamp(offset.x, -1, 1);
                            int zDir = Mathf.Clamp(offset.z, -1, 1);
                            Vector3Int verticalOffset = new Vector3Int(0, offset.y, 0);
                            Vector3Int horizontalOffset = new Vector3Int(xDir, 0, zDir);

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
            List<Vector3Int> result = new List<Vector3Int>();

            while (node != null)
            {
                stack.Push(node.Position);
                node = node.Previous;
            }

            while (stack.Count > 0)
            {
                result.Add(stack.Pop());
            }

            return result;
        }
    }
}