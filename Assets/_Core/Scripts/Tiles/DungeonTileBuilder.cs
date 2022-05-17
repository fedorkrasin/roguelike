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
        }
    }
}