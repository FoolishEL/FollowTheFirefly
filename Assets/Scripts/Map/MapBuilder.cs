using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class MapBuilder
{
    private Vector2Int _size;
    [SerializeField] private TileColliderSetter spriteRenderer;

    [SerializeField] private List<SidedImages> sidedImages;
    private (TileColliderSetter, Sides)[,] _mapVizual;
    private bool isNewMap = true;
    private Transform _spawnTransform;
    private int[,] _map;
    private List<Vector2Int> _mainRoad;
    private Vector2Int _startPosition;
    private Vector2Int _exitPosition;
    private Vector2Int _playerPosition;
    private List<Vector2Int> _playerRoad;


    public void Initialize(Vector2Int size, Transform transform, int[,] map, List<Vector2Int> mainRoad,
        Vector2Int startPosition, Vector2Int exitPosition, Vector2Int playerPosition, List<Vector2Int> playerRoad)
    {
        _size = size;
        _spawnTransform = transform;
        _map = map;
        _mainRoad = new List<Vector2Int>();
        _mainRoad.AddRange(mainRoad);
        _startPosition = startPosition;
        _exitPosition = exitPosition;
        _playerPosition = playerPosition;
        _playerRoad = playerRoad;
    }

    public Vector2Int GetTilePositionByWorldPosition(Vector2 worldPosition)
    {
        Vector2Int result = Vector2Int.zero;
        float distanse = int.MaxValue;
        for (int i = 0; i < _size.x; i++)
        {
            for (int j = 0; j < _size.y; j++)
            {
                if (Vector2.Distance(_mapVizual[i, j].Item1.transform.position, worldPosition) < distanse)
                {
                    distanse = Vector2.Distance(_mapVizual[i, j].Item1.transform.position, worldPosition);
                    result.x = i;
                    result.y = j;
                }
            }
        }
        return result;
    }

    public void Vizualize()
    {
        if (isNewMap)
            _mapVizual = new (TileColliderSetter, Sides)[_size.x, _size.y];
        for (int i = 0; i < _size.x; i++)
        {
            for (int j = 0; j < _size.y; j++)
            {
                if (isNewMap)
                {
                    _mapVizual[i, j] = (GameObject.Instantiate(spriteRenderer, _spawnTransform), Sides.None);
                    _mapVizual[i, j].Item1.transform.position =
                        new Vector3(i * MapGeneratorConstants.POSITION_SCALE_MODIFICATOR,
                            j * MapGeneratorConstants.POSITION_SCALE_MODIFICATOR);
                }
                else
                {
                    _mapVizual[i,j].Item1.SetupColliders(Sides.None);
                    _mapVizual[i, j].Item1.SetTileInfo(0);
                    _mapVizual[i, j].Item1.SpriteRenderer.sprite = null;
                    _mapVizual[i, j].Item2 = Sides.None;
                }

                if (_map[i, j] == MapGeneratorConstants.CORE_PATH_ID)
                {
                    _mapVizual[i, j].Item1.SpriteRenderer.color = Color.yellow;
                }
            }
        }

        AddRoad(_mainRoad);

        if (_playerRoad != null)
        {
            AddRoad(_playerRoad);
        }

        for (int i = 0; i < _size.x; i++)
        {
            for (int j = 0; j < _size.y; j++)
            {
                var sprite = FindSprite(_mapVizual[i,j].Item2);
                if (sprite != null)
                {
                    _mapVizual[i,j].Item1.SpriteRenderer.sprite = sprite;
                    _mapVizual[i,j].Item1.SetupColliders(_mapVizual[i,j].Item2);
                    _mapVizual[i, j].Item1.SetTileInfo(_map[i, j]);
                    _mapVizual[i,j].Item1.SpriteRenderer.color = Color.white;
                }
            }
        }

        _mapVizual[_startPosition.x, _startPosition.y].Item1.SpriteRenderer.color = Color.blue;
        _mapVizual[_exitPosition.x, _exitPosition.y].Item1.SpriteRenderer.color = Color.red;
        _mapVizual[_playerPosition.x, _playerPosition.y].Item1.SpriteRenderer.color = Color.green;
        isNewMap = false;
    }

    private void AddRoad(List<Vector2Int> road)
    {
        for (int i = 0; i < road.Count - 1; i++)
        {
            if ((road[i] - road[i + 1]).Equals(Vector2Int.down))
            {
                _mapVizual[road[i].x, road[i].y].Item2 |= Sides.Down;
                _mapVizual[road[i + 1].x, road[i + 1].y].Item2 |= Sides.Up;
            }

            if ((road[i] - road[i + 1]).Equals(Vector2Int.up))
            {
                _mapVizual[road[i].x, road[i].y].Item2 |= Sides.Up;
                _mapVizual[road[i + 1].x, road[i + 1].y].Item2 |= Sides.Down;
            }

            if ((road[i] - road[i + 1]).Equals(Vector2Int.right))
            {
                _mapVizual[road[i].x, road[i].y].Item2 |= Sides.Right;
                _mapVizual[road[i + 1].x, road[i + 1].y].Item2 |= Sides.Left;
            }

            if ((road[i] - road[i + 1]).Equals(Vector2Int.left))
            {
                _mapVizual[road[i].x, road[i].y].Item2 |= Sides.Left;
                _mapVizual[road[i + 1].x, road[i + 1].y].Item2 |= Sides.Right;
            }
        }
    }

    private Sprite FindSprite(Sides side)
    {
        if (side.Equals(Sides.None))
            return null;
        List<SidedImages> SutibleSidedImage = new List<SidedImages>();
        SutibleSidedImage.AddRange(sidedImages);
        if (side.HasFlag(Sides.Down))
            SutibleSidedImage = SutibleSidedImage.Where(c => c.Side.HasFlag(Sides.Down)).ToList();
        else
        {
            SutibleSidedImage = SutibleSidedImage.Where(c => !c.Side.HasFlag(Sides.Down)).ToList();
        }

        if (side.HasFlag(Sides.Up))
            SutibleSidedImage = SutibleSidedImage.Where(c => c.Side.HasFlag(Sides.Up)).ToList();
        else
        {
            SutibleSidedImage = SutibleSidedImage.Where(c => !c.Side.HasFlag(Sides.Up)).ToList();
        }

        if (side.HasFlag(Sides.Right))
            SutibleSidedImage = SutibleSidedImage.Where(c => c.Side.HasFlag(Sides.Right)).ToList();
        else
        {
            SutibleSidedImage = SutibleSidedImage.Where(c => !c.Side.HasFlag(Sides.Right)).ToList();
        }

        if (side.HasFlag(Sides.Left))
            SutibleSidedImage = SutibleSidedImage.Where(c => c.Side.HasFlag(Sides.Left)).ToList();
        else
        {
            SutibleSidedImage = SutibleSidedImage.Where(c => !c.Side.HasFlag(Sides.Left)).ToList();
        }

        if (SutibleSidedImage.Any())
        {
            return SutibleSidedImage.First().Image;
        }

        if (side == Sides.Down || side == Sides.Up)
        {
            SutibleSidedImage.Clear();
            SutibleSidedImage.AddRange(sidedImages);
            SutibleSidedImage = SutibleSidedImage.Where(c =>
                    c.Side.HasFlag(Sides.Down) && c.Side.HasFlag(Sides.Up) && !c.Side.HasFlag(Sides.Left) &&
                    !c.Side.HasFlag(Sides.Right))
                .ToList();
            if (SutibleSidedImage.Any())
                return SutibleSidedImage.First().Image;
        }

        if (side == Sides.Left || side == Sides.Right)
        {
            SutibleSidedImage.Clear();
            SutibleSidedImage.AddRange(sidedImages);
            SutibleSidedImage = SutibleSidedImage.Where(c =>
                    c.Side.HasFlag(Sides.Right) && c.Side.HasFlag(Sides.Left) && !c.Side.HasFlag(Sides.Down) &&
                    !c.Side.HasFlag(Sides.Up))
                .ToList();
            if (SutibleSidedImage.Any())
                return SutibleSidedImage.First().Image;
        }

        return null;
    }
}

[Serializable]
public struct SidedImages
{
    public Sides Side;
    public Sprite Image;
}

[Flags]
public enum Sides
{
    None = 0,
    Up = 1,
    Down = 2,
    Right = 4,
    Left = 8,
}