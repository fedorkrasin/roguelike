using System.Collections.Generic;
using System.Linq;
using _Core.Scripts.DelaunayTriangulation;
using _Core.Scripts.DungeonPathfinder;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Core.Scripts
{
    public class RoomGenerator : MonoBehaviour
    {
        [SerializeField] private Transform _roomsParent;
        [SerializeField] private Transform _corridorsParent;
        [SerializeField] private Transform _stairsParent;

        [SerializeField] private int _roomsCount;

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

        [Space] [Range(0, 1)] [SerializeField] private float _corridorGeneratingChance;

        [SerializeField] private GameObject _cubePrefab;
        [SerializeField] private Material _whiteMaterial;
        [SerializeField] private Material _redMaterial;
        [SerializeField] private Material _blueMaterial;

        private List<Room> _rooms;
        private List<Corridor> _corridors;
        private Grid<CellType> _grid;
        private bool[,,] _isCellTaken;

        public void Start()
        {
            ClearRooms();
            ClearCorridors();
            GenerateRooms();
            GenerateCorridors();
            CalculateCorridors();
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
            _grid = new Grid<CellType>(new Vector3Int(_mapXSize, _mapYSize, _mapZSize), Vector3Int.zero);

            for (var y = 0; y < _mapYSize; y += _yStep)
            for (var z = 0; z < _mapZSize; z += _zStep)
            for (var x = 0; x < _mapXSize; x += _xStep)
            {
                if (Random.Range(0, 10) != 2) continue;

                var position = new Vector3Int(x, y, z);
                var size = GetRandomRoomSize();

                if (AreCellsEmpty(position, size))
                {
                    Debug.Log($"{position}, {size}");
                    TakeCells(position, size);
                    InstantiateRoom(position, size);
                }
            }
        }

        private void TakeCells(Vector3Int position, Vector3Int size)
        {
            for (var y = position.y - size.y / 2; y <= position.y + size.y / 2; y++)
            for (var z = position.z - size.z / 2; z <= position.z + size.z / 2; z++)
            for (var x = position.x - size.x / 2; x <= position.x + size.x / 2; x++)
            {
                if (x < 0 || y < 0 || z < 0) continue;
                
                if (x < _mapXSize && y < _mapYSize && z < _mapZSize)
                {
                    _grid[x, y, z] = CellType.Room;
                }
            }
        }

        private bool AreCellsEmpty(Vector3Int position, Vector3Int size)
        {
            for (var y = position.y; y <= position.y + size.y; y++)
            for (var z = position.z; z <= position.z + size.z; z++)
            for (var x = position.x; x <= position.x + size.x; x++)
            {
                if (x < _mapXSize && y < _mapYSize && z < _mapZSize)
                {
                    if (_grid[x, y, z] != CellType.None)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void InstantiateRoom(Vector3Int position, Vector3Int size)
        {
            PlaceRoom(position, size);
        }

        private Vector3Int GetRandomRoomSize()
        {
            var x = Random.Range(1, _maxRoomXSize);
            var y = Random.Range(1, _maxRoomYSize);
            var z = Random.Range(1, _maxRoomZSize);
            if (x % 2 == 0) x--;
            if (y % 2 == 0) y--;
            if (z % 2 == 0) z--;
            return new Vector3Int(x, y, z);
        }

        private void GenerateCorridors()
        {
            _corridors = new List<Corridor>();
            
            var roomPoints = _rooms.Select(room => new Point(room.transform.position)).ToList();
            var roomsTriangulation = DelaunayTriangulation.DelaunayTriangulation.Triangulate(roomPoints);
            var roomsMst = MinimumSpanningTree.Build(roomsTriangulation);

            var corridors = new List<Edge>(roomsMst);

            foreach (var edge in corridors)
            {
                var corridor = new GameObject().AddComponent<Corridor>();
                corridor.SetParameters(edge);
                corridor.name = "Corridor";
                corridor.transform.position = edge.Center;
                corridor.transform.parent = _corridorsParent;
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
            var size = new Vector3Int(_mapXSize, _mapYSize, _mapZSize);
            var aStar = new Pathfinder(size);

            foreach (var corridor in _corridors)
            {
                var startRoom = corridor.Start;
                var endRoom = corridor.End;

                var startPos = new Vector3Int((int) startRoom.x, (int) startRoom.y, (int) startRoom.z);
                var endPos = new Vector3Int((int) endRoom.x, (int) endRoom.y, (int) endRoom.z);

                var path = aStar.FindPath(startPos, endPos, (a, b) =>
                {
                    var pathCost = new PathCost();

                    var delta = b.Position - a.Position;

                    if (delta.y == 0)
                    {
                        pathCost.cost = Vector3Int.Distance(b.Position, endPos); //heuristic

                        switch (_grid[b.Position])
                        {
                            case CellType.Stairs:
                                return pathCost;
                            case CellType.Room:
                                pathCost.cost += 5;
                                break;
                            case CellType.None:
                                pathCost.cost += 1;
                                break;
                        }

                        pathCost.traversable = true;
                    }
                    else
                    {
                        if ((_grid[a.Position] != CellType.None && _grid[a.Position] != CellType.Corridor)
                            || (_grid[b.Position] != CellType.None && _grid[b.Position] != CellType.Corridor))
                            return pathCost;

                        pathCost.cost = 100 + Vector3Int.Distance(b.Position, endPos);

                        var xDir = Mathf.Clamp(delta.x, -1, 1);
                        var zDir = Mathf.Clamp(delta.z, -1, 1);
                        var verticalOffset = new Vector3Int(0, delta.y, 0);
                        var horizontalOffset = new Vector3Int(xDir, 0, zDir);

                        if (!_grid.InBounds(a.Position + verticalOffset)
                            || !_grid.InBounds(a.Position + horizontalOffset)
                            || !_grid.InBounds(a.Position + verticalOffset + horizontalOffset))
                        {
                            return pathCost;
                        }

                        if (_grid[a.Position + horizontalOffset] != CellType.None
                            || _grid[a.Position + horizontalOffset * 2] != CellType.None
                            || _grid[a.Position + verticalOffset + horizontalOffset] != CellType.None
                            || _grid[a.Position + verticalOffset + horizontalOffset * 2] != CellType.None)
                        {
                            return pathCost;
                        }

                        pathCost.traversable = true;
                        pathCost.isStairs = true;
                    }

                    return pathCost;
                });

                if (path != null)
                {
                    for (var i = 0; i < path.Count; i++)
                    {
                        var current = path[i];

                        if (_grid[current] == CellType.None)
                        {
                            _grid[current] = CellType.Corridor;
                        }

                        if (i > 0)
                        {
                            var prev = path[i - 1];

                            var delta = current - prev;

                            if (delta.y != 0)
                            {
                                var xDir = Mathf.Clamp(delta.x, -1, 1);
                                var zDir = Mathf.Clamp(delta.z, -1, 1);
                                var verticalOffset = new Vector3Int(0, delta.y, 0);
                                var horizontalOffset = new Vector3Int(xDir, 0, zDir);

                                _grid[prev + horizontalOffset] = CellType.Stairs;
                                _grid[prev + horizontalOffset * 2] = CellType.Stairs;
                                _grid[prev + verticalOffset + horizontalOffset] = CellType.Stairs;
                                _grid[prev + verticalOffset + horizontalOffset * 2] = CellType.Stairs;

                                PlaceStairs(prev + horizontalOffset);
                                PlaceStairs(prev + horizontalOffset * 2);
                                PlaceStairs(prev + verticalOffset + horizontalOffset);
                                PlaceStairs(prev + verticalOffset + horizontalOffset * 2);
                            }
                        }
                    }

                    foreach (var pos in path.Where(pos => _grid[pos] == CellType.Corridor))
                    {
                        PlaceCorridor(pos);
                    }
                }
            }
        }
        
        private GameObject PlaceCube(Vector3Int location, Vector3Int size, Material material)
        {
            var cube = Instantiate(_cubePrefab, location, Quaternion.identity);
            cube.GetComponent<Transform>().localScale = size;
            cube.GetComponent<MeshRenderer>().material = material;
            return cube;
        }

        private void PlaceRoom(Vector3Int location, Vector3Int size)
        {
            var room = PlaceCube(location, size, _whiteMaterial);
            room.transform.parent = _roomsParent;
            room.name = "Room";
            room.AddComponent<Room>().SetSize(size);
            _rooms.Add(room.GetComponent<Room>());
        }

        private void PlaceCorridor(Vector3Int location)
        {
            var corridor = PlaceCube(location, new Vector3Int(1, 1, 1), _blueMaterial);
            corridor.transform.parent = _corridorsParent;
            corridor.name = "Corridor";
        }

        private void PlaceStairs(Vector3Int location)
        {
            var stairs = PlaceCube(location, new Vector3Int(1, 1, 1), _redMaterial);
            stairs.transform.parent = _stairsParent;
            stairs.name = "Stairs";
        }
    }
}
