using System;
using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class TerrainPainter : MonoBehaviour
{
    // SETUP LAYERS
    // Ogni layer dovrà avere almeno la Texture Diffuse
    // 
    // Ordine di visualizzazione:
    // Layer Acqua
    // Layer Sabbia
    // Layer Erba
    // Layer Terra
    // Layer Roccia
    [Header("Terrain Layers")]
    [SerializeField] TerrainLayer waterLayer;
    [SerializeField] TerrainLayer sandLayer;
    [SerializeField] TerrainLayer grassLayer;
    [SerializeField] TerrainLayer dirtLayer;
    [SerializeField] TerrainLayer rockLayer;

    // ALTEZZA PER LAYER
    // L'altezza sarà un range 0-1.
    // Questo range non è l'altezza di unity, ma un'altezza normalizzata in base all'heightmap
    // Se heightmapMultiplier = 0.6 => altezza massima = 600 y
    // Fa in modo che il valore 0.15 => 90 y
    [Header("Soglia Altezza per Layer")]
    [SerializeField, Range(0f, 1f)] float waterThreshold = 0.1f;
    [SerializeField, Range(0f, 1f)] float sandThreshold = 0.18f;
    [SerializeField, Range(0f, 1f)] float grassThreshold = 0.45f;
    [SerializeField, Range(0f, 1f)] float dirtThreshold = 0.6f;
    // Rock Layer viene generato automaticamente

    [Header("Blend")]
    [Tooltip("Larghezza di transizione tra un layer e l'altro. 0 = taglio netto")]
    [SerializeField, Range(0f, 0.1f)] float blendRange = 0.03f;

    private Terrain _terrain;
    private TerrainData _terrainData;

    private void Awake()
    {
        Initialization();
    }

    private void Initialization()
    {
        if (_terrain == null)
            _terrain = GetComponent<Terrain>();
        if (_terrainData == null)
            _terrainData = _terrain.terrainData;
    }

    public void Paint()
    {
        Initialization();

        // FASE 1 Assegna i layer a TerrainData
        _terrainData.terrainLayers = new TerrainLayer[]
        {
            waterLayer, sandLayer, grassLayer, dirtLayer, rockLayer
        };

        // FASE 2 Usa la heightmap generata per applicare la ALPHAMAP
        int resolution = _terrainData.alphamapResolution; // default => 512 2^n

        // FASE 3 per ogni cella calcola i pesi del layer in base all'altezza
        // Array a 3 dimensioni dove le prime due sono la alphaMap e la terza è il numero di layer
        // La somma dei pesi (thershold) deve essere 1 per ogni cella
        float[,,] splatMap = new float[resolution, resolution, _terrainData.terrainLayers.Length];

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float normalizedX = x / (float)(resolution - 1);
                float normalizedY = y / (float)(resolution - 1);
                float height = _terrainData.GetInterpolatedHeight(normalizedX, normalizedY) / _terrainData.size.y; // normalizza in [0-1]

                // Calcola i pesi per 5 layer
                float[] weights = GetWeights(height);

                for (int i = 0; i < _terrainData.terrainLayers.Length; i++)
                {
                    // ciclando per layer
                    // Ogni cella avrà un massimo di 1 come valore
                    splatMap[y, x, i] = weights[i];
                }
            }
        }

        // FASE 4 applica la SPLATMAP (sinonimo alphamap, weightmap) sul terrain
        _terrainData.SetAlphamaps(0, 0, splatMap);
    }

    // InverseLerp => restituisce:
    // 0 se height <= thershold - blendRange
    // 1 se height >= thershold + blendRange
    // lineare se è nel mezzo
    private float[] GetWeights(float height)
    {
        float[] weights = new float[_terrainData.terrainLayers.Length];

        // Acqua
        // Primo piano
        weights[0] = 1 - Mathf.Clamp01(Mathf.InverseLerp(waterThreshold - blendRange, waterThreshold + blendRange, height));

        // Sabbia
        float sandIn = Mathf.Clamp01(Mathf.InverseLerp(waterThreshold - blendRange, waterThreshold + blendRange, height));
        float sandOut = 1 - Mathf.Clamp01(Mathf.InverseLerp(sandThreshold - blendRange, sandThreshold + blendRange, height));

        weights[1] = sandIn * sandOut;

        // Erba
        float grassIn = Mathf.Clamp01(Mathf.InverseLerp(sandThreshold - blendRange, sandThreshold + blendRange, height));
        float grassOut = 1 - Mathf.Clamp01(Mathf.InverseLerp(grassThreshold - blendRange, grassThreshold + blendRange, height));

        weights[2] = grassIn * grassOut;

        // Terra
        float dirtIn = Mathf.Clamp01(Mathf.InverseLerp(grassThreshold - blendRange, grassThreshold + blendRange, height));
        float dirtOut = 1 - Mathf.Clamp01(Mathf.InverseLerp(dirtThreshold - blendRange, dirtThreshold + blendRange, height));

        weights[3] = dirtIn * dirtOut;

        // Roccia
        weights[4] = Mathf.Clamp01(Mathf.InverseLerp(dirtThreshold - blendRange, dirtThreshold + blendRange, height));

        // Normalizzazione di sicurezza
        float sum = 0;
        foreach (var weight in weights)
        {
            sum += weight;
        }

        if (sum > 0)
        {
            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] /= sum;
            }
        }

        return weights;
    }
}
