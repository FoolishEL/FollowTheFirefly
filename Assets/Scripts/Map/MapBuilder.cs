using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class MapBuilder
{
    private Vector2Int _size;
    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [SerializeField] private List<SidedImages> sidedImages;
    private (SpriteRenderer, Sides)[,] _mapVizual;
    private Transform _spawnTransform;
    private int[,] _map;
    private List<Vector2Int> _mainRoad;
    private Vector2Int _startPosition; 
    private Vector2Int _exitPosition;
    private Vector2Int _playerPosition;
    

    public void Initialize(Vector2Int size, Transform transform, int[,] map, List<Vector2Int> mainRoad,
        Vector2Int startPosition, Vector2Int exitPosition, Vector2Int playerPosition)
    {
        _size = size;
        _spawnTransform = transform;
        _map = map;
        _mainRoad = new List<Vector2Int>();
        _mainRoad.AddRange(mainRoad);
        _startPosition = startPosition;
        _exitPosition = exitPosition;
        _playerPosition = playerPosition;
    }

    public void Vizualize()
    {
        _mapVizual = new (SpriteRenderer,Sides)[_size.x, _size.y];
        for (int i = 0; i < _size.x; i++)
        {
            for (int j = 0; j < _size.y; j++)
            {
                _mapVizual[i, j] = (GameObject.Instantiate(spriteRenderer, _spawnTransform),Sides.None);
                _mapVizual[i, j].Item1.transform.position = new Vector3(i, j);
                if (_map[i, j] == MapGeneratorConstants.CORE_PATH_ID)
                {
                    _mapVizual[i, j].Item1.color = Color.yellow;
                }
            }
        }

        for (int i = 0; i < _mainRoad.Count - 1; i++)
        {
            if ((_mainRoad[i] - _mainRoad[i + 1]).Equals(Vector2Int.down))
            {
                _mapVizual[_mainRoad[i].x, _mainRoad[i].y].Item2 |= Sides.Down;
                _mapVizual[_mainRoad[i+1].x, _mainRoad[i+1].y].Item2 |= Sides.Up;
            }

            if ((_mainRoad[i] - _mainRoad[i + 1]).Equals(Vector2Int.up))
            {
                _mapVizual[_mainRoad[i].x, _mainRoad[i].y].Item2 |= Sides.Up;
                _mapVizual[_mainRoad[i+1].x, _mainRoad[i+1].y].Item2 |= Sides.Down;
            }

            if ((_mainRoad[i] - _mainRoad[i + 1]).Equals(Vector2Int.right))
            {
                _mapVizual[_mainRoad[i].x, _mainRoad[i].y].Item2 |= Sides.Right;
                _mapVizual[_mainRoad[i+1].x, _mainRoad[i+1].y].Item2 |= Sides.Left;
            }

            if ((_mainRoad[i] - _mainRoad[i + 1]).Equals(Vector2Int.left))
            {
                _mapVizual[_mainRoad[i].x, _mainRoad[i].y].Item2 |= Sides.Left;
                _mapVizual[_mainRoad[i+1].x, _mainRoad[i+1].y].Item2 |= Sides.Right;
            }
        }

        foreach (var item in _mapVizual)
        {
            var sprite = FindSprite(item.Item2);
            if (sprite != null)
                item.Item1.sprite = sprite;
        }
        _mapVizual[_startPosition.x, _startPosition.y].Item1.color = Color.blue;
        _mapVizual[_exitPosition.x, _exitPosition.y].Item1.color = Color.red;
        _mapVizual[_playerPosition.x, _playerPosition.y].Item1.color = Color.green;
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
    None =0,
    Up =1,
    Down = 2,
    Right =4,
    Left = 8,
}
