using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public static Map instance;

    public BiomePreset[] biomes;
    public GameObject tilePrefab;
    public GameObject tileParent;
    public GameObject mapRoot;

    [Header("Dimensions")]
    public int width = 50;
    public int height = 50;
    public float scale = 1.0f;
    public Vector2 offset;
    public Vector2Int questGenerationRange = new Vector2Int(50, 50);

    [Header("Height Map")]
    public Wave[] heightWaves;
    public float[,] heightMap;

    [Header("Moisture Map")]
    public Wave[] moistureWaves;
    private float[,] moistureMap;

    [Header("Heat Map")]
    public Wave[] heatWaves;
    private float[,] heatMap;

    [Header("Light Map")]
    public Wave[] lightWaves;
    private float[,] lightMap;

    [Header("Filtering")]
    public int filterIterations = 1;
    public int neighbourThreshold = 3;
    public int neighbourDepth = 1;

    [Header("Light")]
    public float delayBetweenShadeSteps = 0.5f;
    public int lightLevels = 5;
    public int initialLightLevel = 2;
    public int lightLevelStep = 1;
    public float lightWaveInfluenceModifier = 0.5f;
    public float initialLightInfluenceModifier = 0.5f;

    [HideInInspector] public Tile[,] tiles;
    private static Dictionary<string, List<Tile>> biomeTilesCache;

    private List<string> nonPresentBiomes = new List<string>();
    private List<MapShadingData> shadingData = new List<MapShadingData>();

    public static int tileCount;

    [HideInInspector] public float averageLightLevelBalance = 0.0f;
    public float AverageLightLevelBalance
    {
        get
        {
            return averageLightLevelBalance;
        }

        set
        {
            averageLightLevelBalance = value;
        }
    }


    void Start()
    {
        instance = this;
        tileCount = width * height;
    }

    void Update()
    {

    }

    public void GenerateMap()
    {
        tiles = new Tile[width, height];
        biomeTilesCache = new Dictionary<string, List<Tile>>(biomes?.Length ?? 0);

        // height map
        heightMap = NoiseGenerator.Generate(width, height, scale, heightWaves, offset);
        // moisture map
        moistureMap = NoiseGenerator.Generate(width, height, scale, moistureWaves, offset);
        // heat map
        heatMap = NoiseGenerator.Generate(width, height, scale, heatWaves, offset);
        // light map
        lightMap = NoiseGenerator.Generate(width, height, scale, lightWaves, offset);

        int halfWidth = width / 2;
        int halfHeight = height / 2;
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                GameObject tileObject = Instantiate(tilePrefab, new Vector3(x - halfWidth, y - halfHeight, 0), Quaternion.identity);
                Tile tile = tileObject.GetComponent<Tile>();
                tile.X = x;
                tile.Y = y;
                tile.XFromEnd = width - tile.X;
                tile.XFromCenter = Mathf.Abs(tile.X - (width / 2));
                tile.selectedBiome = GetBiome(heightMap[x, y], moistureMap[x, y], heatMap[x, y]);
                tileObject.transform.SetParent(tileParent.transform);
                tileObject.GetComponent<SpriteRenderer>().sprite = tile.selectedBiome.GetTileSprite();
                tiles[x, y] = tile;
            }
        }

        tileParent.transform.SetParent(mapRoot.transform, false);

        float initialLightAlpha = initialLightLevel / (float)lightLevels;
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                tiles[x, y].Prepare();
                float tileAlpha = lightWaveInfluenceModifier * lightMap[x, y] + initialLightInfluenceModifier * initialLightAlpha;
                tiles[x, y].SetShadeByAlpha(tileAlpha);
            }
        }

        for (int i = 0; i < filterIterations; ++i)
            FilterGeneratedMap();

        UpdateTileCache();
        RecalculateLightLevel();

        StartCoroutine(RegionShadingIteration());
    }

    public void UpdateTileCache()
    {
        biomeTilesCache.Clear();
        int halfWidth = width / 2;
        int halfHeight = height / 2;
        int halfDesiredWidth = questGenerationRange.x / 2;
        int halfDesiredHeight = questGenerationRange.y / 2;
        foreach (var tile in tiles)
        {
            var currentTileBiomeName = tile.selectedBiome.name;

            int relativeX = Mathf.Abs(tile.X - halfWidth);
            int relativeY = Mathf.Abs(tile.Y - halfHeight);
            if (relativeX >= halfDesiredWidth || relativeY >= halfDesiredHeight)
            {
                continue;
            }

            if (biomeTilesCache.ContainsKey(currentTileBiomeName))
            {
                var cachedTiles = biomeTilesCache[currentTileBiomeName];

                if (!cachedTiles.Contains(tile))
                {
                    cachedTiles.Add(tile);
                }
            }
            else
            {
                var cachedTiles = new List<Tile> { tile };
                biomeTilesCache.Add(currentTileBiomeName, cachedTiles);
            }
        }
    }

    public void RecalculateLightLevel()
    {
        int totalLightLevel = 0;
        foreach (var tile in tiles)
        {
            totalLightLevel += tile.currentLightLevel;
            // Debug.Log($"Tile {tile.X},{tile.Y} light level: {tile.currentLightLevel}");
        }

        // Debug.Log($"Total light level: {totalLightLevel}");
        Debug.Log($"Tile count: {tileCount}");
        Debug.Log($"Light levels: {lightLevels}");
        Debug.Log($"Initial light level: {initialLightLevel}");

        AverageLightLevelBalance = (float)totalLightLevel / (tileCount * lightLevels);
    }

    public void UpdateAverageForTileShift(bool lightShift)
    {
        if (lightShift)
        {
            AverageLightLevelBalance += lightLevelStep / (lightLevels * tileCount);
        }
        else
        {
            AverageLightLevelBalance -= lightLevelStep / (lightLevels * tileCount);
        }
    }

    public void FillAreaWithBiom(string biomeName, int x, int y, int width, int height)
    {
        BiomePreset biome = null;
        foreach (BiomePreset biomeToCheck in biomes)
        {
            if (biomeToCheck.name == biomeName)
            {
                biome = biomeToCheck;
                break;
            }
        }

        if (biome == null)
        {
            Debug.LogError($"No biome with name '{biomeName}' found");
            return;
        }

        for (int i = x; i < x + width; ++i)
        {
            for (int j = y; j < y + height; ++j)
            {
                tiles[i, j].selectedBiome = biome;
                tiles[i, j].GetComponent<SpriteRenderer>().sprite = biome.GetTileSprite();
            }
        }

        UpdateTileCache();
    }

    public Vector3 GetTilePosition(int x, int y)
    {
        return tiles[x, y].transform.position;
    }

    public Vector3 GetTileLocalPosition(int x, int y)
    {
        return tiles[x, y].transform.localPosition;
    }

    void FilterGeneratedMap()
    {
        // iterate over all tiles
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                BiomePreset biome = tiles[x, y].selectedBiome;
                List<Tile> neighbours = new List<Tile>();
                int neighbourCount = 0;

                for (int neighbourX = x - neighbourDepth; neighbourX <= x + neighbourDepth; ++neighbourX)
                {
                    for (int neighbourY = y - neighbourDepth; neighbourY <= y + neighbourDepth; ++neighbourY)
                    {
                        if (neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height)
                        {
                            if (neighbourX != x || neighbourY != y)
                            {
                                neighbours.Add(tiles[neighbourX, neighbourY]);
                                ++neighbourCount;
                            }
                        }
                    }
                }

                int myBiomeCount = 0;
                foreach (Tile neighbour in neighbours)
                {
                    if (neighbour != null && neighbour.selectedBiome == biome)
                        ++myBiomeCount;
                }

                if (myBiomeCount < neighbourThreshold)
                {
                    // change biome
                    BiomePreset newBiome = null;
                    int newBiomeCount = 0;
                    foreach (BiomePreset biomeToCheck in biomes)
                    {
                        int biomeToCheckCount = 0;
                        foreach (Tile neighbour in neighbours)
                        {
                            if (neighbour != null && neighbour.selectedBiome == biomeToCheck)
                                ++biomeToCheckCount;
                        }

                        if (biomeToCheckCount > newBiomeCount)
                        {
                            newBiome = biomeToCheck;
                            newBiomeCount = biomeToCheckCount;
                        }
                    }

                    tiles[x, y].selectedBiome = newBiome;
                    tiles[x, y].GetComponent<SpriteRenderer>().sprite = newBiome.GetTileSprite();
                }
            }
        }
    }

    BiomePreset GetBiome(float height, float moisture, float heat)
    {
        List<BiomeTempData> biomeTemp = new List<BiomeTempData>();
        foreach (BiomePreset biome in biomes)
        {
            if (biome.MatchCondition(height, moisture, heat))
            {
                biomeTemp.Add(new BiomeTempData(biome));
            }
        }

        BiomePreset biomeToReturn = null;
        float curVal = 0.0f;
        foreach (BiomeTempData biome in biomeTemp)
        {
            if (biomeToReturn == null)
            {
                biomeToReturn = biome.biome;
                curVal = biome.GetDiffValue(height, moisture, heat);
            }
            else
            {
                if (biome.GetDiffValue(height, moisture, heat) < curVal)
                {
                    biomeToReturn = biome.biome;
                    curVal = biome.GetDiffValue(height, moisture, heat);
                }
            }
        }

        if (biomeToReturn == null)
            biomeToReturn = biomes[0];
        return biomeToReturn;
    }

    public static List<Tile> GetBiomeTiles(params string[] biomes)
    {
        if (biomes == null)
        {
            throw new System.Exception("Empty biomes array provided");
        }

        var result = new List<Tile>();

        foreach (string biome in biomes)
        {
            if (instance.nonPresentBiomes.Contains(biome))
            {
                continue;
            }
            if (!biomeTilesCache.ContainsKey(biome))
            {
                Debug.LogWarning($"No tiles for '{biome}' found");
                instance.nonPresentBiomes.Add(biome);
                continue;
            }

            result.AddRange(biomeTilesCache[biome]);
        }

        return result;
    }

    public void ShowMap()
    {
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                tiles[x, y].Show();
            }
        }
    }

    public void HideMap()
    {
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                tiles[x, y].Hide();
            }
        }
    }

    public void ChangeRegionShade(int x, int y, int radius, ShadeType shadeType = ShadeType.Neutral)
    {
        if(x < 0 || y < 0 || radius < 0)
        {
            throw new System.Exception($"Invalid x or y or radius data provided: x={x},y={y},radius={radius}");
        }

        // due to the quest item size we need to center the whole zone
        y--;

        var centerTile = tiles[x, y];
        SetTileShade(centerTile, shadeType);

        MapShadingData data = new MapShadingData(x, y, radius, shadeType);
        shadingData.Add(data);
    }

    private void SetTileShade(Tile tile, ShadeType shadeType)
    {
        switch (shadeType)
        {
            case ShadeType.Dark:
                tile.AddDarkShade();
                break;
            case ShadeType.Light:
                tile.AddLightShade();
                break;
            case ShadeType.Neutral:
                tile.ResetShade();
                break;
        }
    }

    IEnumerator RegionShadingIteration()
    {
        while(true)
        {
            List<MapShadingData> shadingDataToForget = new List<MapShadingData>();
            foreach (var data in shadingData)
            {
                if (data.tilesToProcess.Count == 0)
                {
                    shadingDataToForget.Add(data);
                    continue;
                }

                List<Tile> radiusTiles = data.tilesToProcess[0];
                foreach (var tile in radiusTiles)
                {
                    SetTileShade(tile, data.shadeType);
                }
                data.tilesToProcess.RemoveAt(0);
            }

            foreach (var data in shadingDataToForget)
            {
                shadingData.Remove(data);
            }

            yield return new WaitForSeconds(delayBetweenShadeSteps);
        }
    }
}

public class MapShadingData
{
    public int x;
    public int y;
    public int radius;
    public ShadeType shadeType;
    public MapShadingData(int x, int y, int radius, ShadeType shadeType)
    {
        this.x = x;
        this.y = y;
        this.radius = radius;
        this.shadeType = shadeType;

        const double threshold = 0.8;

        List<Tile> processedTiles = new List<Tile>();
        for (int r = 1; r <= radius; r++)
        {
            List<Tile> radiusTiles = new List<Tile>();
            tilesToProcess.Add(radiusTiles);
            for (int i = Math.Max(0, x - r - 1); i <= Math.Min(Map.instance.width - 1, x + r + 1); i++)
            {
                for (int j = Math.Max(0, y - r - 1); j <= Math.Min(Map.instance.height - 1, y + r + 1); j++)
                {
                    var distance = Math.Sqrt(Math.Pow(i - x, 2) + Math.Pow(j - y, 2));
                    var coverage = Math.Min(1, Math.Max(0, r + threshold - distance));
                    var selectedTile = Map.instance.tiles[i, j];
                    if (coverage > 0 && !processedTiles.Contains(selectedTile))
                    {
                        radiusTiles.Add(selectedTile);
                        processedTiles.Add(selectedTile);
                    }
                }
            }
        }
    }

    public List<List<Tile>> tilesToProcess = new List<List<Tile>>();
}

public class BiomeTempData
{
    public BiomePreset biome;
    public BiomeTempData(BiomePreset preset)
    {
        biome = preset;
    }

    public float GetDiffValue(float height, float moisture, float heat)
    {
        return (height - biome.minHeight) + (moisture - biome.minMoisture) + (heat - biome.minHeat);
    }
}

[System.Serializable]
public enum ShadeType
{
    Dark,
    Light,
    Neutral
}
