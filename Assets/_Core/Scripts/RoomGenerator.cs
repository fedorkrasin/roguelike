using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    [SerializeField] private int _xSize;
    [SerializeField] private int _ySize;
    [SerializeField] private int _zSize;

    private List<Room> _rooms;
    private bool[,,] _isCellTaken;

    public void Start()
    {
        ClearRooms();
        InitializeRoomsList();
        InitializeTakenCellsArray();
        GenerateRooms();
    }

    private void InitializeRoomsList()
    {
        _rooms = new List<Room>();
    }

    private void InitializeTakenCellsArray()
    {
        _isCellTaken = new bool[_xSize, _ySize, _zSize];
    }

    private void GenerateRooms()
    {
        for (var y = 0; y < _ySize; y++)
        {
            for (var z = 0; z < _zSize; z+=7)
            {
                for (var x = 0; x < _xSize; x+=7)
                {
                    if (Random.Range(0, 10) == 2)
                    {
                        var xSize = Random.Range(1, 8);
                        var ySize = Random.Range(1, 4);
                        var zSize = Random.Range(1, 8);

                        var position = new Vector3(x, y, z);
                        var size = new Vector3(xSize, ySize, zSize);
                        
                        if (AreCellsEmpty(position, size))
                        {
                            TakeCells(position, size);

                            var room = new GameObject().AddComponent<Room>();
                            room.gameObject.name = "Room";
                            room.transform.position = position;

                            room.SetSize(size);
                            room.InstantiateRoom();
                            
                            _rooms.Add(room);
                        }
                    }
                }
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
        foreach (var room in _rooms.Where(room => room != null))
        {
            DestroyImmediate(room.gameObject);
        }

        _rooms.Clear();
    }
}
