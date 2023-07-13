using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DeBroglie.Constraints;
using Tessera;
using UnityEngine;
using CountConstraint = Tessera.CountConstraint;
using PathConstraint = Tessera.PathConstraint;
using SeparationConstraint = Tessera.SeparationConstraint;

public class TesseraGeneratorWrapper : MonoBehaviour
{
    [SerializeField] private MapGenerationOptions mapGenerationOptions;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private TesseraGenerator tesseraGenerator;
    [SerializeField] private TesseraSquareTile emptyTile;
    
    [SerializeField] private MonsterSpawner monsterSpawner;
    [SerializeField] private LightController lightController;
    private bool isFirstTime;
    private StationaryTile entranceTile;
    private StationaryTile exitTile;
    private CountConstraint entranceCountConstraint;
    private CountConstraint exitCountConstraint;
    private CountConstraint compassCountConstraint;
    private List<TesseraTileBase> lightenedTiles;

    public bool IsFirstTime => isFirstTime || (Application.isEditor && !Application.isPlaying);
    //GameManager.Instance.hasCompas
    private void Awake()
    {
        lightenedTiles = new List<TesseraTileBase>();
        CompasTrigger.onCompasPicked += OnCompassPicked;
        isFirstTime = true;
        CreateConstrainRules();
        Generate();
        FindStationaryTiles();
        isFirstTime = false;
        monsterSpawner.SpawnMonsters();
        playerTransform.position = entranceTile.transform.position;
        lightController.AddStaticLight(entranceTile.transform);
        StartCoroutine(RespawnMaze());
    }

    private void OnDestroy()
    {
        CompasTrigger.onCompasPicked -= OnCompassPicked;
    }

    [ContextMenu(nameof(PrepareGeneration))]
    private void PrepareGeneration()
    {
        if(isFirstTime)
            return;
        var areas = lightController.GetWalkableArea(out var range);
        range *= 2f;
        var notLightened = lightenedTiles.Where(wasLightened =>
            areas.All(lightPoint => Vector2.Distance(lightPoint, wasLightened.transform.position) > range)).ToList();
        lightenedTiles.RemoveAll(c => notLightened.Contains(c));
        notLightened.ForEach(c => c.transform.SetParent(tesseraGenerator.transform));
        foreach (var tileUnderGenerator in tesseraGenerator.GetComponentsInChildren<TesseraTileBase>())
        {
            if(tileUnderGenerator == exitTile || tileUnderGenerator == entranceTile)
                continue;
            if (areas.Any(c => Vector2.Distance(c, (Vector3)tileUnderGenerator.transform.position) <= range))
            {
                tileUnderGenerator.transform.SetParent(transform);
                lightenedTiles.Add(tileUnderGenerator);
            }
        }
    }
    [ContextMenu(nameof(Generate))]
    private void Generate()
    {
        if (!Application.isPlaying) CreateConstrainRules();
        
        PrepareGeneration();
        tesseraGenerator.Clear();
        if (IsFirstTime)
        {
            List<TesseraSquareTile> outsideBlocks = new List<TesseraSquareTile>();
            SurroundWithBlocks(new Vector2Int(tesseraGenerator.size.x, tesseraGenerator.size.y), outsideBlocks);
            if (!Application.isPlaying)
            {
                foreach (var tesseraSquareTile in outsideBlocks)
                {
                    tesseraSquareTile.transform.SetParent(tesseraGenerator.transform);
                }
            }
            outsideBlocks.Clear();
        }
        tesseraGenerator.Generate();
    }
    private void SurroundWithBlocks(Vector2Int size, List<TesseraSquareTile> outsideBlocks)
    {
        float xSizeStart = MapGeneratorConstants.POSITION_SCALE_MODIFICATOR * (size.x - 1) / 2;
        float xSize = xSizeStart;
        float ySizeStart = MapGeneratorConstants.POSITION_SCALE_MODIFICATOR * (size.y - 1) / 2;
        float ySize = ySizeStart;
        Vector3 initialPosition = tesseraGenerator.transform.position;
        for (int x = 0; x < size.x; x++)
        {
            outsideBlocks.Add(
                Instantiate(emptyTile, initialPosition + new Vector3(xSize, ySizeStart), Quaternion.identity,
                    transform));
            xSize -= MapGeneratorConstants.POSITION_SCALE_MODIFICATOR;
        }
        ySize = ySizeStart - MapGeneratorConstants.POSITION_SCALE_MODIFICATOR * (size.y - 1);
        xSize = xSizeStart;
        for (int x = 0; x < size.x; x++)
        {
            outsideBlocks.Add(
                Instantiate(emptyTile, initialPosition + new Vector3(xSize, ySize), Quaternion.identity, transform));
            xSize -= MapGeneratorConstants.POSITION_SCALE_MODIFICATOR;
        }
        ySize = ySizeStart - MapGeneratorConstants.POSITION_SCALE_MODIFICATOR;
        for (int y = 1; y < size.y - 1; y++)
        {
            outsideBlocks.Add(
                Instantiate(emptyTile, initialPosition + new Vector3(xSizeStart, ySize), Quaternion.identity,
                    transform));
            ySize -= MapGeneratorConstants.POSITION_SCALE_MODIFICATOR;
        }
        ySize = ySizeStart - MapGeneratorConstants.POSITION_SCALE_MODIFICATOR;
        xSize += MapGeneratorConstants.POSITION_SCALE_MODIFICATOR;
        for (int y = 1; y < size.y - 1; y++)
        {
            outsideBlocks.Add(
                Instantiate(emptyTile, initialPosition + new Vector3(xSize, ySize), Quaternion.identity, transform));
            ySize -= MapGeneratorConstants.POSITION_SCALE_MODIFICATOR;
        }
    }
    private void FindStationaryTiles()
    {
        var stationary = tesseraGenerator.gameObject.GetComponentsInChildren<StationaryTile>();
        if (stationary.Length != 2)
        {
            throw new ArgumentException("wrong stationary objects Count!");
        }
        entranceTile = stationary[0].Type == StationaryTileType.Entrance ? stationary[0] : stationary[1];
        exitTile = stationary[1].Type == StationaryTileType.Exit ? stationary[1] : stationary[0];
        entranceTile.transform.SetParent(transform);
        exitTile.transform.SetParent(transform);
        //TODO: possible remove from generation list!
        entranceCountConstraint.count = 0;
        exitCountConstraint.count = 0;
    }

    private IEnumerator RespawnMaze()
    {
        WaitForSeconds awaitor = new WaitForSeconds(mapGenerationOptions.RegenerationTime);
        while (isActiveAndEnabled && GameManager.Instance.isPlaying)
        {
            yield return awaitor;
            if (isActiveAndEnabled && GameManager.Instance.isPlaying)
                Generate();
        }
    }

    private void CreateConstrainRules()
    {
        var constrains = tesseraGenerator.GetComponents<TesseraConstraint>().ToList();
        constrains = constrains.Where(c => c is not PathConstraint).ToList();
#if UNITY_EDITOR
        if (Application.isPlaying)
            constrains.ForEach(Destroy);
        else
            constrains.ForEach(DestroyImmediate);
#else
        constrains.ForEach(Destroy);
#endif
        var size = mapGenerationOptions.Size + Vector2Int.one * 2;
        tesseraGenerator.size = new Vector3Int(size.x, size.y);
        tesseraGenerator.tiles = mapGenerationOptions.GetTileEntries();
        entranceCountConstraint = AddSingleConstrains(mapGenerationOptions.entrances.tileBases);
        exitCountConstraint = AddSingleConstrains(mapGenerationOptions.exits.tileBases);
        compassCountConstraint = AddSingleConstrains(mapGenerationOptions.compasses.tileBases);
        AddSeparation(mapGenerationOptions.exits.tileBases.Concat(mapGenerationOptions.entrances.tileBases),
            mapGenerationOptions.MinimalDistanceToExit);

        CountConstraint AddSingleConstrains(List<TesseraTileBase> tiles)
        {
            if (tiles.Count == 0)
                return null;
            var countConstraint = tesseraGenerator.gameObject.AddComponent<CountConstraint>();
            countConstraint.count = 1;
            countConstraint.comparison = CountComparison.Exactly;
            countConstraint.tiles = tiles;
            return countConstraint;
        }

        void AddSeparation(IEnumerable<TesseraTileBase> tiles, int minimalSeparation)
        {
            var separator = tesseraGenerator.gameObject.AddComponent<SeparationConstraint>();
            separator.tiles = new List<TesseraTileBase>(tiles);
            separator.minDistance = minimalSeparation;
        }
    }

    private void OnCompassPicked()
    {
        CompasTrigger.onCompasPicked -= OnCompassPicked;
        //TODO possible remove from gen list!
        compassCountConstraint.count = 0;
    }
}
