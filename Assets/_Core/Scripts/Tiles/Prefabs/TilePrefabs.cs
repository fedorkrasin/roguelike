using UnityEngine;

namespace _Core.Scripts.Tiles.Prefabs
{
    public class TilePrefabs : ScriptableObject
    {
        [SerializeField] private GameObject _roomPrefab;
        [SerializeField] private GameObject _corridorPrefab;
        [SerializeField] private GameObject _stairsPrefab;

        public GameObject RoomPrefab => _roomPrefab;
        public GameObject CorridorPrefab => _corridorPrefab;
        public GameObject StairsPrefab => _stairsPrefab;
    }
}