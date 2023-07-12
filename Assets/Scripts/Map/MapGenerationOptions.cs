using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tessera;
using UnityEngine;

[CreateAssetMenu(menuName = "MapGenerationOptions",fileName = "MapGenerationOptions")]
public class MapGenerationOptions : ScriptableObject
{
    public Vector2Int Size = Vector2Int.one * 12;
    public float RegenerationTime = 10f;
    [SerializeField]
    private List<TilesWithWeight> roads;
    public TilesWithWeight entrances;
    public TilesWithWeight exits;
    public TilesWithWeight compasses;
    public int MinimalDistanceToExit = 10;

    public IEnumerable<TilesWithWeight> allTiles => roads.Append(entrances).Append(exits).Append(compasses);

    public List<TileEntry> GetTileEntries()
    {
        List<TileEntry> result = new List<TileEntry>();
        foreach (var tilesWithWeight in allTiles)
        {
            foreach (var tile in tilesWithWeight.tileBases)
            {
                result.Add(new TileEntry() { tile = tile, weight = tilesWithWeight.Weight });
            }
        }
        return result;
    }


    [Serializable]
    public struct TilesWithWeight
    {
        public float Weight;
        public List<TesseraTileBase> tileBases;
    }
}
