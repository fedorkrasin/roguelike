using _Core.Scripts.DelaunayTriangulation;
using UnityEngine;

namespace _Core.Scripts.DungeonCells
{
    public class Corridor : MonoBehaviour
    {
        private Vector3 _start;
        private Vector3 _end;

        public Vector3 Start => _start;
        public Vector3 End => _end;

        private float HeightDifference => Mathf.Abs(_start.y - _end.y);

        public void SetParameters(Edge edge)
        {
            _start = edge.Point1.Position;
            _end = edge.Point2.Position;
        }

        public void InstantiateCorridor()
        {

        }

        private void BuildCorridor()
        {

        }
    }
}