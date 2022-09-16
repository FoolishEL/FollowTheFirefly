using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private Vector2Int size = Vector2Int.one * 6;
    [SerializeField] private Vector2Int startPosition;
    [SerializeField] private Vector2Int exitPosition;
    [SerializeField] private Vector2Int playerPosition;

    [SerializeField] private int corePointsCount = 3;

    [SerializeField] private SpriteRenderer spriteRenderer;
    private List<Vector2Int> corePoint;
    private List<Vector2Int> mainRoad;

    private int[,] map;
    private int[,] connectionMap;
    private int[,] tempMap;
    private int[,] paths;
    private SpriteRenderer[,] mapVizual;
    private int mappingSize;

    private void Awake()
    {
        GenerateMap();
    }

    [ContextMenu(nameof(GenerateMap))]
    private void GenerateMap()
    {
        DestroyAllChildren();
        map = new int[size.x, size.y];
        mappingSize = size.x * size.y;
        connectionMap = new int[mappingSize, mappingSize];
        tempMap = new int[mappingSize, mappingSize];
        paths = new int[mappingSize, mappingSize];
        map[startPosition.x, startPosition.y] = MapGeneratorConstants.START_ID;
        map[exitPosition.x, exitPosition.y] = MapGeneratorConstants.EXIT_ID;
        map[playerPosition.x, playerPosition.y] = MapGeneratorConstants.PLAYER_ID;
        CreateRoads();
        Visualze();
    }
    private void CreateRoads()
    {
        corePoint = new List<Vector2Int>();
        for (int i = 0; i < corePointsCount; i++)
        {
            var randomVector = Random.insideUnitCircle;
            randomVector.x = Mathf.Abs(randomVector.x);
            randomVector.y = Mathf.Abs(randomVector.y);
            randomVector.x *= size.x;
            randomVector.y *= size.y;
            var randomPosition = new Vector2Int((int)randomVector.x, (int)randomVector.y);
            if (corePoint.Any(c => c.Equals(randomPosition)) || randomPosition.Equals(startPosition) ||
                randomPosition.Equals(playerPosition) || randomPosition.Equals(exitPosition))
            {
                i--;
                continue;
            }

            if (corePoint.Any(c => Vector2Int.Distance(c, randomPosition) < 2f) ||
                Vector2Int.Distance(startPosition, randomPosition) < 2f ||
                Vector2Int.Distance(playerPosition, randomPosition) < 2f ||
                Vector2Int.Distance(exitPosition, randomPosition) < 2f)
            {
                i--;
                continue; 
            }
            corePoint.Add(randomPosition);
            map[corePoint[i].x, corePoint[i].y] = MapGeneratorConstants.CORE_POINT;
        }

        PrepareMapping();
        GeneratePath();
    }
    private void PrepareMapping()
    {
        for (int i = 0; i < mappingSize; i++)
        {
            for (int j = 0; j < size.x * size.y; j++)
            {
                connectionMap[i, j] = Int32.MaxValue/3;
            }
        }

        for (int i = 0; i < mappingSize; i++)
        {
            var position = GetPositionById(i);
            if (position.x + 1 < size.x)
            {
                connectionMap[i, GetIdByPosition(position.x + 1, position.y)] = 1;
            }

            if (position.x - 1 >= 0)
            {
                connectionMap[i, GetIdByPosition(position.x - 1, position.y)] = 1;
            }

            if (position.y + 1 < size.y)
            {
                connectionMap[i, GetIdByPosition(position.x, position.y + 1)] = 1;
            }

            if (position.y - 1 >= 0)
            {
                connectionMap[i, GetIdByPosition(position.x, position.y - 1)] = 1;
            }
        }
        //TODO: optional!
        for (int i = 0; i < corePoint.Count; i++)
        {
            for (int j = 0; j < corePoint.Count; j++)
            {
                //TODO: optional!
                connectionMap[GetIdByPosition(corePoint[i].x, corePoint[i].y),
                    GetIdByPosition(corePoint[j].x, corePoint[j].y)] = Int32.MaxValue/3;
            }
        }
    }
    private void RegenerateDxtra()
    {
        for (int i = 0; i < mappingSize; i++)
        {
            for (int j = 0; j < mappingSize; j++)
            {
                tempMap[i, j] = connectionMap[i, j];
                paths[i, j] = connectionMap[i, j] == Int32.MaxValue/3 ? -1 : j;
            }
        }
        for (int k = 0; k < mappingSize; k++)
        for (int i = 0; i < mappingSize; i++)
        for (int j = 0; j < mappingSize; j++)
        {
            if (tempMap[i, j] > tempMap[i, k] + tempMap[k, j])
            {
                tempMap[i, j] = tempMap[i, k] + tempMap[k, j];
                paths[i, j] = paths[i, k];
            }
        }
    }
    private void GeneratePath()
    {
        RegenerateDxtra();
        mainRoad = new List<Vector2Int>();
        List<Vector2Int> roadPart = new List<Vector2Int>();
        roadPart = GetPath(startPosition, corePoint[0]);
        mainRoad.AddRange(roadPart);
        EncloseRoad(roadPart);
        RegenerateDxtra();
        for (int i = 0; i < corePoint.Count - 1; i++)
        {
            roadPart = GetPath(corePoint[i], corePoint[i + 1]);
            roadPart.RemoveAt(0);
            mainRoad.AddRange(roadPart);
            EncloseRoad(roadPart);
            RegenerateDxtra();
        }
        roadPart = GetPath(corePoint.Last(), exitPosition);
        roadPart.RemoveAt(0);
        mainRoad.AddRange(roadPart);
    }

    private void EncloseRoad(List<Vector2Int> road)
    {
        connectionMap[GetIdByPosition(road[1].x, road[1].y), GetIdByPosition(road[0].x , road[0].y)] =
            int.MaxValue / 3;
        for (int i = 1; i < road.Count - 2; i++)
        {
            // connectionMap[GetIdByPosition(road[i].x, road[i].y), GetIdByPosition(road[i - 1].x, road[i - 1].y)] =
            //     Int32.MaxValue / 3;
            if (road[i].x + 1 < size.x)
            {
                connectionMap[GetIdByPosition(road[i].x, road[i].y), GetIdByPosition(road[i].x + 1, road[i].y)] =
                    int.MaxValue / 3;
                connectionMap[GetIdByPosition(road[i].x + 1, road[i].y), GetIdByPosition(road[i].x, road[i].y)] =
                    int.MaxValue / 3;
            }

            if (road[i].x - 1 >= 0)
            {
                connectionMap[GetIdByPosition(road[i].x, road[i].y), GetIdByPosition(road[i].x - 1, road[i].y)] =
                    int.MaxValue / 3;
                connectionMap[GetIdByPosition(road[i].x - 1, road[i].y), GetIdByPosition(road[i].x, road[i].y)] =
                    int.MaxValue / 3;
            }

            if (road[i].y + 1 < size.y)
            {
                connectionMap[GetIdByPosition(road[i].x, road[i].y), GetIdByPosition(road[i].x, road[i].y + 1)] =
                    int.MaxValue / 3;
                connectionMap[GetIdByPosition(road[i].x, road[i].y + 1), GetIdByPosition(road[i].x, road[i].y)] =
                    int.MaxValue / 3;
            }

            if (road[i].y - 1 >= 0)
            {
                connectionMap[GetIdByPosition(road[i].x, road[i].y), GetIdByPosition(road[i].x, road[i].y - 1)] =
                    int.MaxValue / 3;
                connectionMap[GetIdByPosition(road[i].x, road[i].y - 1), GetIdByPosition(road[i].x, road[i].y)] =
                    int.MaxValue / 3;
            }

            connectionMap[GetIdByPosition(road[^1].x, road[^1].y), GetIdByPosition(road[^2].x, road[^2].y)] =
                int.MaxValue / 3;
        }
    }

    private Vector2Int GetPositionById(int id)
    {
        var x = id / size.y;
        var y = id % size.y;
        return new Vector2Int(x, y);
    }

    private int GetIdByPosition(int x, int y)
    {
        return x * size.y + y;
    }

    private void Visualze()
    {
        mapVizual = new SpriteRenderer[size.x, size.y];
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                mapVizual[i, j] = Instantiate(spriteRenderer, transform);
                mapVizual[i, j].transform.position = new Vector3(i, j);
            }
        }

        for (int i = 0; i < corePoint.Count; i++)
        {
            mapVizual[corePoint[i].x, corePoint[i].y].color = Color.yellow;
        }

        mapVizual[startPosition.x, startPosition.y].color = Color.blue;
        mapVizual[exitPosition.x, exitPosition.y].color = Color.red;
        mapVizual[playerPosition.x, playerPosition.y].color = Color.green;
    }

    [ContextMenu(nameof(DestroyAllChildren))]
    private void DestroyAllChildren()
    {
        var children = GetComponentsInChildren<Transform>(false).ToList();
        children.RemoveAt(0);
        foreach (var item in children)
        {
            DestroyImmediate(item.gameObject);
        }
    }

    [SerializeField] private bool showPath = false;
    private void OnDrawGizmos()
    {
        if(mainRoad==null)
            return;
        Handles.color = Color.black;
        if (!showPath)
        {
            for (int i = 0; i < mappingSize; i++)
            {
                for (int j = 0; j < mappingSize; j++)
                {
                    if (connectionMap[i, j] == 1)
                    {
                        Vector2 center = Vector2.Lerp(GetPositionById(i), GetPositionById(j), .5f);
                        Handles.DrawSolidDisc(center, Vector3.forward, .05f);
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < mainRoad.Count-1; i++)
            {
                Vector2 center = Vector2.Lerp(mainRoad[i], mainRoad[i + 1], .5f);
                Handles.Label(center, i.ToString());
            }
        }
    }

    private List<Vector2Int> GetPath(Vector2Int start,Vector2Int end)
    {
        int position = GetIdByPosition(start.x, start.y);
        int endPosition = GetIdByPosition(end.x, end.y);
        List<Vector2Int> result = new();
        if (tempMap[position, endPosition] == Int32.MaxValue/3)
        {
            return result;
        }
        while (position!=endPosition)
        {
            result.Add(GetPositionById(position));
            position = paths[position, endPosition];
        }
        result.Add(GetPositionById(endPosition));
        return result;
    }
}