using System;
using System.Collections.Generic;
using System.Linq;
using _Core.Scripts.DelaunayTriangulation;
using _Core.Scripts.DungeonCells;
using _Core.Scripts.DungeonPathfinder;
using _Core.Scripts.Tiles;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Core.Scripts
{
    public class DungeonGenerator : MonoBehaviour
    {
        [Header("Map")]
        [SerializeField] private Vector3Int _mapSize;
        [SerializeField] private Vector3Int _generatingStep;

        [Header("Rooms")] 
        [SerializeField] private Vector3Int _minRoomSize;
        [SerializeField] private Vector3Int _maxRoomSize;

        [Space] [Range(0, 1)] [SerializeField] private float _corridorGeneratingChance;

        [SerializeField] private TileBuilders _mode;
        [SerializeField] private CubeTileBuilder _cubeTileBuilder;
        [SerializeField] private DungeonTileBuilder _dungeonTileBuilder;

        private TileBuilder _tileBuilder;
        private List<Room> _rooms;
        private List<Corridor> _corridors;
        private List<Stairs> _stairs;
        private Grid<CellType> _grid;
        private List<Edge> _corridorEdges;

        public void Start()
        {
            ChooseBuilder();
            ClearRooms();
            ClearCorridors();
            GenerateRooms();
            GenerateCorridors();
            CalculateCorridors();
        }

        private void ChooseBuilder()
        {
            _tileBuilder = _mode switch
            {
                TileBuilders.Cube => _cubeTileBuilder,
                TileBuilders.Dungeon => _dungeonTileBuilder
            };
        }

        private void GenerateRooms()
        {
            _rooms = new List<Room>();
            _grid = new Grid<CellType>(new Vector3Int(_mapSize.x, _mapSize.y, _mapSize.z), Vector3Int.zero);

            for (var y = 0; y < _mapSize.y; y += _generatingStep.y)
            for (var z = 0; z < _mapSize.z; z += _generatingStep.z)
            for (var x = 0; x < _mapSize.x; x += _generatingStep.x)
            {
                if (Random.Range(0, 10) != 2) continue;

                var position = new Vector3Int(x, y, z);
                var size = GetRandomRoomSize();

                if (AreCellsEmpty(position, size))
                {
                    TakeCells(position, size);
                    _tileBuilder.PlaceRoom(position, size, _rooms);
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
                
                if (x < _mapSize.x && y < _mapSize.y && z < _mapSize.z)
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
                if (x < _mapSize.x && y < _mapSize.y && z < _mapSize.z)
                {
                    if (_grid[x, y, z] != CellType.None)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private Vector3Int GetRandomRoomSize()
        {
            var x = Random.Range(_minRoomSize.x, _maxRoomSize.x);
            var y = Random.Range(_minRoomSize.y, _maxRoomSize.y);
            var z = Random.Range(_minRoomSize.z, _maxRoomSize.z);
            if (x % 2 == 0) x--;
            if (y % 2 == 0) y--;
            if (z % 2 == 0) z--;
            return new Vector3Int(x, y, z);
        }

        private void GenerateCorridors()
        {
            _corridors = new List<Corridor>();
            _stairs = new List<Stairs>();
            
            var roomPoints = _rooms.Select(room => new Point(room.transform.position)).ToList();
            var roomsTriangulation = DelaunayTriangulation.DelaunayTriangulation.Triangulate(roomPoints);
            var roomsMst = MinimumSpanningTree.Build(roomsTriangulation);
            
            _corridorEdges = new List<Edge>(roomsMst);

            foreach (var edge in roomsTriangulation.Edges)
            {
                if (Random.Range(0f, 1f) < _corridorGeneratingChance)
                {
                    var inverseEdge = new Edge(edge.Point1, edge.Point2);

                    if (!_corridorEdges.Contains(edge) && !_corridorEdges.Contains(inverseEdge))
                    {
                        _corridorEdges.Add(edge);
                    }
                }
            }
        }

        public void ClearMap()
        {
            ClearRooms();
            ClearCorridors();
            ClearStairs();
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

        private void ClearStairs()
        {
            if (_stairs != null)
            {
                foreach (var stairs in _stairs.Where(stairs => stairs != null))
                {
                    DestroyImmediate(stairs.gameObject);
                }

                _stairs.Clear();
            }
        }

        private void CalculateCorridors()
        {
            var size = new Vector3Int(_mapSize.x, _mapSize.y, _mapSize.z);
            var aStar = new Pathfinder(size);

            foreach (var corridor in _corridorEdges)
            {
                var startRoom = corridor.Point1;
                var endRoom = corridor.Point2;

                var startPos = new Vector3Int((int) startRoom.X, (int) startRoom.Y, (int) startRoom.Z);
                var endPos = new Vector3Int((int) endRoom.X, (int) endRoom.Y, (int) endRoom.Z);

                var path = aStar.FindPath(startPos, endPos, (a, b) =>
                {
                    var pathCost = new PathCost();

                    var delta = b.Position - a.Position;

                    if (delta.y == 0)
                    {
                        pathCost.cost = Vector3Int.Distance(b.Position, endPos);

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
                        if (_grid[a.Position] != CellType.None && _grid[a.Position] != CellType.Corridor
                            || _grid[b.Position] != CellType.None && _grid[b.Position] != CellType.Corridor)
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

                                _tileBuilder.PlaceStairs(prev + horizontalOffset, _stairs);
                                _tileBuilder.PlaceStairs(prev + horizontalOffset * 2, _stairs);
                                _tileBuilder.PlaceStairs(prev + verticalOffset + horizontalOffset, _stairs);
                                _tileBuilder.PlaceStairs(prev + verticalOffset + horizontalOffset * 2, _stairs);
                            }
                        }
                    }

                    foreach (var pos in path.Where(pos => _grid[pos] == CellType.Corridor))
                    {
                        _tileBuilder.PlaceCorridor(pos, _corridors);
                    }
                }
            }
        }
    }
}
