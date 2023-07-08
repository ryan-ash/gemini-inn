using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public BiomePreset[] biomes;
    public GameObject tilePrefab;

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

    // Start is called before the first frame update
    void Start()
    {
        GenerateMap();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GenerateMap ()
    {
        // height map
        heightMap = NoiseGenerator.Generate(width, height, scale, heightWaves, offset);
        // moisture map
        moistureMap = NoiseGenerator.Generate(width, height, scale, moistureWaves, offset);
        // heat map
        heatMap = NoiseGenerator.Generate(width, height, scale, heatWaves, offset);

        int halfWidth = width / 2;
        int halfHeight = height / 2;

        for(int x = 0; x < width; ++x)
        {
            for(int y = 0; y < height; ++y)
            {
                GameObject tile = Instantiate(tilePrefab, new Vector3(x - halfWidth, y - halfHeight, 0), Quaternion.identity);
                tile.transform.SetParent(transform);
                tile.GetComponent<SpriteRenderer>().sprite = GetBiome(heightMap[x, y], moistureMap[x, y], heatMap[x, y]).GetTleSprite();
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