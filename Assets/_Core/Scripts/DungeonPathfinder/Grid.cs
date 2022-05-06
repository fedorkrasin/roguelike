using UnityEngine;

namespace _Core.Scripts.DungeonPathfinder
{
    public class Grid<T>
    {
        private readonly T[] _data;

        public Vector3Int Size { get; private set; }
        public Vector3Int Offset { get; set; }

        public Grid(Vector3Int size, Vector3Int offset)
        {
            Size = size;
            Offset = offset;

            _data = new T[size.x * size.y * size.z];
        }

        public int GetIndex(Vector3Int pos)
        {
            return pos.x + (Size.x * pos.y) + (Size.x * Size.y * pos.z);
        }

        public bool InBounds(Vector3Int pos)
        {
            return new BoundsInt(Vector3Int.zero, Size).Contains(pos + Offset);
        }

        public T this[int x, int y, int z]
        {
            get => this[new Vector3Int(x, y, z)];
            set => this[new Vector3Int(x, y, z)] = value;
        }

        public T this[Vector3Int pos]
        {
            get
            {
                pos += Offset;
                return _data[GetIndex(pos)];
            }
            set
            {
                pos += Offset;
                _data[GetIndex(pos)] = value;
            }
        }
    }
}