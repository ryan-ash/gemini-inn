using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public BiomePreset[] biomes;
    public GameObject tilePrefab;
    public GameObject tileParent;

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

        // height map
        heightMap = NoiseGenerator.Generate(width, height, scale, heightWaves, offset);
        // moisture map
        moistureMap = NoiseGenerator.Generate(width, height, scale, moistureWaves, offset);
        // heat map
        heatMap = NoiseGenerator.Generate(width, height, scale, heatWaves, offset);

        int halfWidth = width / 2;
        int halfHeight = height / 2;
        int totalTiles = width * height;

        int tileNumber = 0;
        for (int x = 0; x < width; ++x)
        {
            for(int y = 0; y < height; ++y)
            {
                GameObject tileObject = Instantiate(tilePrefab, new Vector3(x - halfWidth, y - halfHeight, 0), Quaternion.identity);
                Tile tile = tileObject.GetComponent<Tile>();
                tile.N = tileNumber++;
                tile.NFromEnd = totalTiles - tile.N;
                tile.selectedBiome = GetBiome(heightMap[x, y], moistureMap[x, y], heatMap[x, y]);
                tile.Prepare();
                tileObject.transform.SetParent(tileParent.transform);
                tileObject.GetComponent<SpriteRenderer>().sprite = tile.selectedBiome.GetTileSprite();
                tiles[x, y] = tile;
            }
        }

        for (int i = 0; i < filterIterations; ++i)
            FilterGeneratedMap();
    }

    void FilterGeneratedMap()
    {
        // iterate over all tiles
        for (int x = 0; x < width; ++x)
        {
            for(int y = 0; y < height; ++y)
            {
                BiomePreset biome = tiles[x, y].selectedBiome;
                List<Tile> neighbours = new List<Tile>();
                int neighbourCount = 0;

                for (int neighbourX = x - neighbourDepth; neighbourX <= x + neighbourDepth; ++neighbourX)
                {
                    for (int neighbourY = y - neighbourDepth; neighbourY <= y + neighbourDepth; ++neighbourY)
                    {
                        if(neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height)
                        {
                            if(neighbourX != x || neighbourY != y)
                            {
                                neighbours.Add(tiles[neighbourX, neighbourY]);
                                ++neighbourCount;
                            }
                        }
                    }
                }

                int myBiomeCount = 0;
                foreach(Tile neighbour in neighbours)
                {
                    if(neighbour != null && neighbour.selectedBiome == biome)
                        ++myBiomeCount;
                }

                if(myBiomeCount < neighbourThreshold)
                {
                    // change biome
                    BiomePreset newBiome = null;
                    int newBiomeCount = 0;
                    foreach(BiomePreset biomeToCheck in biomes)
                    {
                        int biomeToCheckCount = 0;
                        foreach(Tile neighbour in neighbours)
                        {
                            if(neighbour != null && neighbour.selectedBiome == biomeToCheck)
                                ++biomeToCheckCount;
                        }

                        if(biomeToCheckCount > newBiomeCount)
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

    BiomePreset GetBiome (float height, float moisture, float heat)
    {
        List<BiomeTempData> biomeTemp = new List<BiomeTempData>();
        foreach(BiomePreset biome in biomes)
        {
            if(biome.MatchCondition(height, moisture, heat))
            {
                biomeTemp.Add(new BiomeTempData(biome));                
            }
        }

        BiomePreset biomeToReturn = null;
        float curVal = 0.0f;
        foreach(BiomeTempData biome in biomeTemp)
        {
            if(biomeToReturn == null)
            {
                biomeToReturn = biome.biome;
                curVal = biome.GetDiffValue(height, moisture, heat);
            }
            else
            {
                if(biome.GetDiffValue(height, moisture, heat) < curVal)
                {
                    biomeToReturn = biome.biome;
                    curVal = biome.GetDiffValue(height, moisture, heat);
                }
            }
        }

        if(biomeToReturn == null)
            biomeToReturn = biomes[0];
        return biomeToReturn;
    }
}

public class BiomeTempData
{
    public BiomePreset biome;
    public BiomeTempData (BiomePreset preset)
    {
        biome = preset;
    }
        
    public float GetDiffValue (float height, float moisture, float heat)
    {
        return (height - biome.minHeight) + (moisture - biome.minMoisture) + (heat - biome.minHeat);
    }
}
