using System;
using System.Collections.Generic;
using System.Linq;
using _Core.Scripts;
using _Core.Scripts.DelaunayTriangulation;
using _Core.Scripts.DungeonPathfinder;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomGenerator : MonoBehaviour
{
    [SerializeField] private Transform _roomsParent;
    [SerializeField] private Transform _corridorsParent;
    
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

    [Space]
    [Range(0, 1)]
    [SerializeField] private float _corridorGeneratingChance;

    private List<Room> _rooms;
    private List<Corridor> _corridors;
    private bool[,,] _isCellTaken;

    public void Start()
    {
        ClearRooms();
        ClearCorridors();
        GenerateRooms();
        GenerateCorridors();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        // foreach (var t in triangulation.Triangles)
        // {
        //     Gizmos.DrawLine(t.Vertices[0].Position, t.Vertices[1].Position);
        //     Gizmos.DrawLine(t.Vertices[0].Position, t.Vertices[2].Position);
        //     Gizmos.DrawLine(t.Vertices[1].Position, t.Vertices[2].Position);
        // }
        
        // foreach (var edge in mst)
        // {
        //     Gizmos.DrawLine(edge.Point1.Position, edge.Point2.Position);
        // }

        foreach (var corridor in _corridors)
        {
            Gizmos.DrawLine(corridor.Start, corridor.End);
        }
    }
    
    private void GenerateRooms()
    {
        _rooms = new List<Room>();
        _isCellTaken = new bool[_mapXSize, _mapYSize, _mapZSize];
        
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

    private void GenerateCorridors()
    {
        var roomPoints = _rooms.Select(room => new Point(room.transform.position)).ToList();
        var roomsTriangulation = DelaunayTriangulation.Triangulate(roomPoints);
        var roomsMst = MinimumSpanningTree.Build(roomsTriangulation);

        var corridors = new List<Edge>(roomsMst);

        foreach (var edge in corridors)
        {
            var corridor = new GameObject().AddComponent<Corridor>();
            corridor.name = "Corridor";
            corridor.transform.position = edge.Center;
            corridor.transform.parent = _corridorsParent;
            corridor.SetParameters(edge);
            _corridors.Add(corridor);
        }

        foreach (var edge in roomsTriangulation.Edges)
        {
            if (Random.Range(0f, 1f) < _corridorGeneratingChance)
            {
                var inverseEdge = new Edge(edge.Point1, edge.Point2);

                if (!corridors.Contains(edge) && !corridors.Contains(inverseEdge))
                {
                    corridors.Add(edge);
                    var corridor = new GameObject().AddComponent<Corridor>();
                    corridor.name = "Corridor";
                    corridor.transform.position = edge.Center;
                    corridor.transform.parent = _corridorsParent;
                    corridor.SetParameters(edge);
                    _corridors.Add(corridor);
                }
            }
        }
    }
    
    public void ClearMap()
    {
        ClearRooms();
        ClearCorridors();
    }

    private void ClearRooms()
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

    private void ClearCorridors()
    {
        if (_corridors != null)
        {
            foreach (var corridor in _corridors.Where(corridor => corridor != null))
            {
                DestroyImmediate(corridor.gameObject);
            }

            _corridors.Clear();
        }
    }
    
    private void CalculateCorridors() 
    {
        var aStar = new Pathfinder(size);

        foreach (var corridor in _corridors) {
            var startRoom = (corridor.Start as Vertex<Room>).Item;
            var endRoom = (corridor.End as Vertex<Room>).Item;

            var startPosf = startRoom.bounds.center;
            var endPosf = endRoom.bounds.center;
            var startPos = new Vector3Int((int)startPosf.x, (int)startPosf.y, (int)startPosf.z);
            var endPos = new Vector3Int((int)endPosf.x, (int)endPosf.y, (int)endPosf.z);

            var path = aStar.FindPath(startPos, endPos, (Node a, Node b) => {
                var pathCost = new PathCost();

                var delta = b.Position - a.Position;

                if (delta.y == 0) {
                    //flat hallway
                    pathCost.cost = Vector3Int.Distance(b.Position, endPos);    //heuristic

                    if (grid[b.Position] == CellType.Stairs) {
                        return pathCost;
                    } else if (grid[b.Position] == CellType.Room) {
                        pathCost.cost += 5;
                    } else if (grid[b.Position] == CellType.None) {
                        pathCost.cost += 1;
                    }

                    pathCost.traversable = true;
                } else {
                    //staircase
                    if ((grid[a.Position] != CellType.None && grid[a.Position] != CellType.Corridor)
                        || (grid[b.Position] != CellType.None && grid[b.Position] != CellType.Corridor)) return pathCost;

                    pathCost.cost = 100 + Vector3Int.Distance(b.Position, endPos);    //base cost + heuristic

                    int xDir = Mathf.Clamp(delta.x, -1, 1);
                    int zDir = Mathf.Clamp(delta.z, -1, 1);
                    Vector3Int verticalOffset = new Vector3Int(0, delta.y, 0);
                    Vector3Int horizontalOffset = new Vector3Int(xDir, 0, zDir);

                    if (!grid.InBounds(a.Position + verticalOffset)
                        || !grid.InBounds(a.Position + horizontalOffset)
                        || !grid.InBounds(a.Position + verticalOffset + horizontalOffset)) {
                        return pathCost;
                    }

                    if (grid[a.Position + horizontalOffset] != CellType.None
                        || grid[a.Position + horizontalOffset * 2] != CellType.None
                        || grid[a.Position + verticalOffset + horizontalOffset] != CellType.None
                        || grid[a.Position + verticalOffset + horizontalOffset * 2] != CellType.None) {
                        return pathCost;
                    }

                    pathCost.traversable = true;
                    pathCost.isStairs = true;
                }

                return pathCost;
            });

            if (path != null) {
                for (int i = 0; i < path.Count; i++) {
                    var current = path[i];

                    if (grid[current] == CellType.None) {
                        grid[current] = CellType.Hallway;
                    }

                    if (i > 0) {
                        var prev = path[i - 1];

                        var delta = current - prev;

                        if (delta.y != 0) {
                            int xDir = Mathf.Clamp(delta.x, -1, 1);
                            int zDir = Mathf.Clamp(delta.z, -1, 1);
                            Vector3Int verticalOffset = new Vector3Int(0, delta.y, 0);
                            Vector3Int horizontalOffset = new Vector3Int(xDir, 0, zDir);
                            
                            grid[prev + horizontalOffset] = CellType.Stairs;
                            grid[prev + horizontalOffset * 2] = CellType.Stairs;
                            grid[prev + verticalOffset + horizontalOffset] = CellType.Stairs;
                            grid[prev + verticalOffset + horizontalOffset * 2] = CellType.Stairs;

                            PlaceStairs(prev + horizontalOffset);
                            PlaceStairs(prev + horizontalOffset * 2);
                            PlaceStairs(prev + verticalOffset + horizontalOffset);
                            PlaceStairs(prev + verticalOffset + horizontalOffset * 2);
                        }

                        Debug.DrawLine(prev + new Vector3(0.5f, 0.5f, 0.5f), current + new Vector3(0.5f, 0.5f, 0.5f), Color.blue, 100, false);
                    }
                }

                foreach (var pos in path) {
                    if (grid[pos] == CellType.Hallway) {
                        PlaceHallway(pos);
                    }
                }
            }
        }
        
        void PlaceCube(Vector3Int location, Vector3Int size, Material material) 
        {
            var go = Instantiate(cubePrefab, location, Quaternion.identity);
            go.GetComponent<Transform>().localScale = size;
            go.GetComponent<MeshRenderer>().material = material;
        }

        void PlaceRoom(Vector3Int location, Vector3Int size) {
            PlaceCube(location, size, redMaterial);
        }

        void PlaceHallway(Vector3Int location) {
            PlaceCube(location, new Vector3Int(1, 1, 1), blueMaterial);
        }

        void PlaceStairs(Vector3Int location) {
            PlaceCube(location, new Vector3Int(1, 1, 1), greenMaterial);
        }
    }
}
