using UnityEngine;

namespace _Core.Scripts.DungeonCells
{
    public class Room : MonoBehaviour
    {
        private int _xSize;
        private int _ySize;
        private int _zSize;

        private GameObject _room;

        public void SetSize(Vector3 size)
        {
            _xSize = (int) size.x;
            _ySize = (int) size.y;
            _zSize = (int) size.z;
        }

        public void InstantiateRoom()
        {
            var roomSize = new Vector3(_xSize, _ySize, _zSize);
            _room = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _room.transform.localScale = roomSize;
            _room.transform.parent = transform;
            _room.transform.localPosition = Vector3.zero;
        }
    }
}
