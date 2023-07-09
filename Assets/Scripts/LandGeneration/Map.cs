using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public BiomePreset[] biomes;
    public GameObject tilePrefab;
    public GameObject tileParent;
    public GameObject mapRoot;

    [Header("Dimensions")]
    public int width = 50;
    public int height = 50;
    public float scale = 1.0f;
    public Vector2 offset;

    [Header("Height Map")]
    public Wave[] heightWaves;
    public float[,] heightMap;

    [Header("Moisture Map")]
    public Wave[] moistureWaves;
    private float[,] moistureMap;

    [Header("Heat Map")]
    public Wave[] heatWaves;
    private float[,] heatMap;

    [Header("Filtering")]
    public int filterIterations = 1;
    public int neighbourThreshold = 3;
    public int neighbourDepth = 1;

    private Tile[,] tiles;
    private static Dictionary<string, List<Tile>> biomeTilesCache;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
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

        int halfWidth = width / 2;
        int halfHeight = height / 2;
        int totalTiles = width;
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                GameObject tileObject = Instantiate(tilePrefab, new Vector3(x - halfWidth, y - halfHeight, 0), Quaternion.identity);
                Tile tile = tileObject.GetComponent<Tile>();
                tile.X = x;
                tile.Y = y;
                tile.XFromEnd = totalTiles - tile.X;
                tile.XFromCenter = Mathf.Abs(tile.X - (totalTiles / 2));
                tile.selectedBiome = GetBiome(heightMap[x, y], moistureMap[x, y], heatMap[x, y]);
                tileObject.transform.SetParent(tileParent.transform);
                tileObject.GetComponent<SpriteRenderer>().sprite = tile.selectedBiome.GetTileSprite();
                tiles[x, y] = tile;
            }
        }

        tileParent.transform.SetParent(mapRoot.transform, false);

        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                tiles[x, y].Prepare();
            }
        }

        for (int i = 0; i < filterIterations; ++i)
            FilterGeneratedMap();

        foreach (var tile in tiles)
        {
            var currentTileBiomeName = tile.selectedBiome.name;

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

    public static IReadOnlyList<Tile> GetBiomeTiles(params string[] biomes)
    {
        if (biomes == null)
        {
            throw new System.Exception("Empty biomes array provided");
        }

        var result = new List<Tile>();

        foreach (string biome in biomes)
        {
            if (string.IsNullOrWhiteSpace(biome) || !biomeTilesCache.ContainsKey(biome))
            {
                throw new System.Exception($"Incorrect biome name - '{biome}'");
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
