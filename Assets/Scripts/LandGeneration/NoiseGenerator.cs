using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseGenerator : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public static float[,] Generate (int width, int height, float scale, Wave[] waves, Vector2 offset)
    {
        float[,] noiseMap = new float[width, height];

        List<float> seedOffsets = new List<float>();
        List<float> frequencyOffsets = new List<float>();
        List<float> amplitudeOffsets = new List<float>();

        for (int i = 0; i < waves.Length; i++)
        {
            Wave wave = waves[i];
            float seed = wave.seed + Random.Range(-wave.seedRandomness, wave.seedRandomness);
            float frequency = wave.frequency + Random.Range(-wave.frequencyRandomness, wave.frequencyRandomness);
            float amplitude = wave.amplitude + Random.Range(-wave.amplitudeRandomness, wave.amplitudeRandomness);

            seedOffsets.Add(seed);
            frequencyOffsets.Add(frequency);
            amplitudeOffsets.Add(amplitude);
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float samplePosX = (float)x * scale + offset.x;
                float samplePosY = (float)y * scale + offset.y;

                float normalization = 0.0f;
                for (int i = 0; i < waves.Length; i++)
                {
                    Wave wave = waves[i];
                    float seedOffset = seedOffsets[i];
                    float frequencyOffset = frequencyOffsets[i];
                    float amplitudeOffset = amplitudeOffsets[i];

                    float seed = wave.seed + seedOffset;
                    float frequency = wave.frequency + frequencyOffset;
                    float amplitude = wave.amplitude + amplitudeOffset;

                    noiseMap[x, y] += amplitude * Mathf.PerlinNoise(samplePosX * frequency + seed, samplePosY * frequency + seed);
                    normalization += amplitude;
                }
                noiseMap[x, y] /= normalization;
            }
        }

        return noiseMap;
    }
}

[System.Serializable]
public class Wave
{
    public float seed;
    public float seedRandomness;
    public float frequency;
    public float frequencyRandomness;
    public float amplitude;
    public float amplitudeRandomness;
}
