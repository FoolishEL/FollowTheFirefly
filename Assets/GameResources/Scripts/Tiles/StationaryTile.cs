using UnityEngine;

public class StationaryTile : MonoBehaviour
{
    [SerializeField] private StationaryTileType type;
    public StationaryTileType Type => type;
}

public enum StationaryTileType
{
    Entrance,
    Exit
}
