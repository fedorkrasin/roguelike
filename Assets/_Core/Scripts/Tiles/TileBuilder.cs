using System.Collections.Generic;
using _Core.Scripts.DungeonCells;
using _Core.Scripts.Tiles.Prefabs;
using UnityEngine;

namespace _Core.Scripts.Tiles
{
    public abstract class TileBuilder : MonoBehaviour
    {
        protected const string roomName = "Room";
        protected const string corridorName = "Corridor";
        protected const string stairsName = "Stairs";
        
        [SerializeField] protected Transform _roomsParent;
        [SerializeField] protected Transform _corridorsParent;
        [SerializeField] protected Transform _stairsParent;
        [SerializeField] protected TilePrefabs _prefabs;

        private static GameObject PlaceCube(Vector3Int location, Vector3Int size, GameObject prefab)
        {
            var cube = Instantiate(prefab, location, Quaternion.identity);
            cube.GetComponent<Transform>().localScale = size;
            return cube;
        }
        
        public virtual void PlaceRoom(Vector3Int location, Vector3Int size, List<Room> list)
        {
            var room = PlaceCube(location, size, _prefabs.RoomPrefab);
            room.transform.parent = _roomsParent;
            room.name = roomName;
            room.AddComponent<Room>().SetSize(size);
            list.Add(room.GetComponent<Room>());
        }

        public virtual void PlaceCorridor(Vector3Int location, List<Corridor> list)
        {
            var corridor = PlaceCube(location, new Vector3Int(1, 1, 1), _prefabs.CorridorPrefab);
            corridor.transform.parent = _corridorsParent;
            corridor.name = corridorName;
            list.Add(corridor.AddComponent<Corridor>());
        }

        public virtual void PlaceStairs(Vector3Int location, List<Stairs> list)
        {
            var stairs = PlaceCube(location, new Vector3Int(1, 1, 1), _prefabs.StairsPrefab);
            stairs.transform.parent = _stairsParent;
            stairs.name = stairsName;
            list.Add(stairs.AddComponent<Stairs>());
        }
    }
}