using System.Collections.Generic;
using _Core.Scripts.DungeonCells;
using UnityEngine;

namespace _Core.Scripts.Tiles
{
    public class DungeonTileBuilder : TileBuilder
    {
        public override void PlaceCorridor(Vector3Int location, List<Corridor> list)
        {
            var corridor = Instantiate(_prefabs.CorridorPrefab, location, Quaternion.identity);
            corridor.transform.parent = _corridorsParent;
            corridor.name = corridorName;
            list.Add(corridor.AddComponent<Corridor>());
        }

        public override void PlaceStairs(Vector3Int location, List<Stairs> list)
        {
            var stairs = Instantiate(_prefabs.StairsPrefab, location, Quaternion.identity);
            stairs.transform.parent = _stairsParent;
            stairs.name = stairsName;
            list.Add(stairs.AddComponent<Stairs>());
        }
    }
}