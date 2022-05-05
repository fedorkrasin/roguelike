using System.Collections.Generic;
using UnityEngine;

namespace _Core.Scripts.DungeonPathfinder
{
    public class Node
    {
        public Vector3Int Position { get; private set; }
        public Node Previous { get; set; }
        public HashSet<Vector3Int> PreviousSet { get; private set; }
        public float Cost { get; set; }

        public Node(Vector3Int position)
        {
            Position = position;
            PreviousSet = new HashSet<Vector3Int>();
        }
    }
}