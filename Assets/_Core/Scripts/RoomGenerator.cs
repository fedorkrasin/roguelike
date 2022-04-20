using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomGenerator : MonoBehaviour
{
    [SerializeField] private Transform _roomsParent;
    
    [Header("Map")]
    [SerializeField] private int _mapXSize;
    [SerializeField] private int _mapYSize;
    [SerializeField] private int _mapZSize;

    [Header("Generating Step")] 
    [SerializeField] private int _xStep;
    [SerializeField] private int _yStep;
    [SerializeField] private int _zStep;

    [Header("Rooms")]
    [SerializeField] private int _maxRoomXSize;
    [SerializeField] private int _maxRoomYSize;
    [SerializeField] private int _maxRoomZSize;

    private List<Room> _rooms;
    private bool[,,] _isCellTaken;

    public void Start()
    {
        ClearRooms();
        InitializeRoomsList();
        InitializeTakenCellsArray();
        GenerateRooms();

        var points = _rooms.Select(room => new Point(room.transform.position)).ToList();
        Debug.Log(points.Count);
        
        var triangles = DelaunayTriangulator.BowyerWatson(points);
        Debug.Log(triangles.Count);
        
        var i = 0;
        foreach (var t in triangles)
        {
            Debug.Log($"triangle: {t.Vertices[0]}; {t.Vertices[1]}; {t.Vertices[2]}");
        }
    }

    // private void OnDrawGizmosSelected()
    // {
    //     var points = _rooms.Select(room => new Point(room.transform.position)).ToList();
    //     Debug.Log(points.Count);
    //     
    //     var triangles = DelaunayTriangulator.BowyerWatson(points);
    //     Debug.Log(triangles.Count);
    //     
    //     var i = 0;
    //     foreach (var t in triangles)
    //     {
    //         Debug.Log($"triangle: {t.Vertices[0]}; {t.Vertices[1]}; {t.Vertices[2]}");
    //         Gizmos.DrawLine(t.Vertices[0].ToVector3(), t.Vertices[1].ToVector3());
    //         Gizmos.DrawLine(t.Vertices[0].ToVector3(), t.Vertices[2].ToVector3());
    //         Gizmos.DrawLine(t.Vertices[1].ToVector3(), t.Vertices[2].ToVector3());
    //     }
    // }

    private void InitializeRoomsList()
    {
        _rooms = new List<Room>();
    }

    private void InitializeTakenCellsArray()
    {
        _isCellTaken = new bool[_mapXSize, _mapYSize, _mapZSize];
    }

    private void GenerateRooms()
    {
        for (var y = 0; y < _mapYSize; y += _yStep)
        for (var z = 0; z < _mapZSize; z += _zStep)
        for (var x = 0; x < _mapXSize; x += _xStep)
        {
            if (Random.Range(0, 10) != 2) continue;
            
            var position = new Vector3(x, y, z);
            var size = GetRandomRoomSize();

            if (AreCellsEmpty(position, size))
            {
                TakeCells(position, size);
                InstantiateRoom(position, size);
            }
        }
    }

    private void TakeCells(Vector3 position, Vector3 size)
    {
        for (var y = (int) position.y; y <= position.y + size.y; y++)
        for (var z = (int) position.z; z <= position.z + size.z; z++)
        for (var x = (int) position.x; x <= position.x + size.x; x++)
        {
            if (x < _isCellTaken.GetLength(0) && y < _isCellTaken.GetLength(1) && z < _isCellTaken.GetLength(2))
            {
                _isCellTaken[x, y, z] = true;
            }
        }
    }

    private bool AreCellsEmpty(Vector3 position, Vector3 size)
    {
        for (var y = (int) position.y; y <= position.y + size.y; y++)
        for (var z = (int) position.z; z <= position.z + size.z; z++)
        for (var x = (int) position.x; x <= position.x + size.x; x++)
        {
            if (x < _isCellTaken.GetLength(0) && y < _isCellTaken.GetLength(1) && z < _isCellTaken.GetLength(2))
            {
                if (_isCellTaken[x, y, z])
                {
                    return false;
                }
            }
        }

        return true;
    }

    public void ClearRooms()
    {
        if (_rooms != null)
        {
            foreach (var room in _rooms.Where(room => room != null))
            {
                DestroyImmediate(room.gameObject);
            }

            _rooms.Clear();
        }
    }

    private void InstantiateRoom(Vector3 position, Vector3 size)
    {
        var room = new GameObject().AddComponent<Room>();
        room.gameObject.name = "Room";
        room.transform.position = position;
        room.transform.parent = _roomsParent;
        room.SetSize(size);
        room.InstantiateRoom();
        _rooms.Add(room);
    }

    private Vector3 GetRandomRoomSize()
    {
        return new Vector3(Random.Range(1, _maxRoomXSize), Random.Range(1, _maxRoomYSize),
            Random.Range(1, _maxRoomZSize));
    }
}
