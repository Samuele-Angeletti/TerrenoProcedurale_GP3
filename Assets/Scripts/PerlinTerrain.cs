using System;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Genera proceduralmente l'heightmap di un terrain usando Perlin Noise
/// Prevede multi-ottava con erosione e falloff opzionali
/// </summary>
[RequireComponent(typeof(Terrain))]
public class PerlinTerrain : MonoBehaviour
{
    // RESOLUTION
    // l'heightmap è una griglia di valori float [0..1]
    // rappresenta l'altezza del terreno. e la risoluzione (resolution) è il numero di celle per lato

    // ATTENZIONE: Unity richiedere che la risoluzione sia sempre 2^n +1
    // => 128 + 1 = 129
    // => 512 + 1 = 513
    // => 2048 + 1 = 2049
    [Header("Terrain Base")]
    [Tooltip("Risoluzione dell'heightmap. Deve essere 2^n+1 (es. 129, 513, 2049...)")]
    [SerializeField] int resolution = 513;

    // NOISE
    // Parametri che modificano il comportamento del noise e quindi l'effetto finale

    [Header("Noise Settings")]
    [Tooltip("Scala del noise. Più alto = montagne larghe. Più basso = colline fitte")]
    [SerializeField, Range(1f, 500f)] float scale = 80;

    // OCTAVES
    // Quanti strati di noise sommiamo
    // Ottava 1: forma generale
    // Ottava 2: colline medie
    // Ottava 3: piccoli dossi
    // Ottava 4: dettaglio
    // ...
    [Tooltip("Numero di strati di noise sovrapposti")]
    [SerializeField, Range(1, 8)] int octaves = 3;

    // PERSISTANCE
    // Controlla di quanto si riduce l'ampiezza di ogni ottava
    // persistance = 0.9 => ogni ottava ha quasi la totale influenza della precedente
    // persistance = 0.5 => ogni ottava ha la metà dell'influenza della precedente
    // persistance = 0.1 => ogni ottava ha poca influenza dalla precedente (terreno morbido e liscio)
    [Tooltip("Riduzione dell'ampiezza per ogni ottava (0 = tutto piatto, 1 = tutte le ottave pesano uguale, caos totale)")]
    [SerializeField, Range(0f, 1f)] float persistance = 0.4f;

    // LACUNARITY
    // Controlla quanto il noise si stringe per ogni ottava
    // lacunarity = 2 => ogni ottava ha frequenza doppia
    // lacunarity = 1.5 => le ottave si moltiplicano più lentamente
    // lacunarity = 3.5 => le ottave diventano subito molto dettagliate
    [Tooltip("Moltiplicatore della frequenza per ogni ottava. Più è alto e più i dettagli sono fitti")]
    [SerializeField, Range(1f, 4f)] float lacunarity = 2f;

    // OFFSET
    // Sposta il punto di campionamento del noise
    // Il noise è DETERMINISTICO, cioè stesse coordinate = stesso risultato
    // L'offset ti permette di spostarti lungo la mappa senza cambiare il seed
    // Utile per animazioni o trovare zone interessanti, belle
    [Tooltip("Sposta il campionamento del noise nello spazio. Utile per esplorare zone diverse della mappa")]
    [SerializeField] Vector2 offset = Vector2.zero;

    // SEED
    [Header("Seed Settings")]
    [Tooltip("Se attivo, viene ignorato il seed")]
    [SerializeField] bool randomSeed = false;
    [Tooltip("Seed per riprodurre la stessa mappa")]
    [SerializeField] int seed = 43;

    // HEIGHT CURVE
    // Curva delle altezze. 
    // Il noise produce valori distribuiti in modo quasi uniforme tra 0 e 1.
    // Questo significa: tanti punti a media altezza, ma pochi alle estremità
    // la Height Curve permette di RIDISTRIBUIRE le altezze
    [Header("Height Curve")]
    [Tooltip("Curva applicata al noise. Asse X = valore noise (0, 1). Asse Y = valore risultante (0, 1)")]
    [SerializeField] AnimationCurve heightCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [Tooltip("Scala finale dell'altezza come frazione di Terrain.y. Ad esempio 0.6 = al 60% dell'altezza massima")]
    [SerializeField, Range(0f, 1f)] float heightMultiplier = 0.6f;

    // FALLOFF
    // Il noise puro non ha bordi.
    // La falloff map è una maschera che va a sottrarre 0-1 all'heightmap
    // EFFETTO ISOLA
    [Header("Falloff Settings")]
    [SerializeField] bool useFalloff = false;
    [Tooltip("Quanto è ripida la discesa ai bordi. Valori alti = scogliere nette")]
    [SerializeField, Range(1f, 5f)] float falloffStrenght = 2.5f;
    [Tooltip("Quanto è grande la zona pianeggiante al centro, prima che inizi la discesa")]
    [SerializeField, Range(1f, 5f)] float falloffShift = 2.5f;

    // EROSION
    // Il noise matematico puro rende i pendii irrealistici.
    // Con l'erosione andiamo a renderli più realistici
    // (questa è una simulazione semplificata, il costo è molto elevato)
    [Header("Erosion Settings")]
    [SerializeField] bool applyErosion = false;
    [Tooltip("Il numero di volte che andiamo ad riapplicare l'erosione. Più alto = terreno più levigato")]
    [SerializeField, Range(1, 20)] int erosionIteration = 5;
    [Tooltip("La forza dell'erosione. Più basso = erosione aggressiva")]
    [SerializeField, Range(0f, 0.1f)] float erosionStrenght = 0.02f;

    private Terrain _terrain;
    private TerrainData _terrainData;

    private void Awake()
    {
        Initialization();
    }

    private void Initialization()
    {
        _terrain = GetComponent<Terrain>();
        _terrainData = _terrain.terrainData;
    }

    private void Start()
    {
        Generate();
    }

    /// <summary>
    /// Funzione di generazione del terreno usando il Perlin Noise.
    /// Calcola la struttura del noise e lo applica al terreno
    /// </summary>
    [ContextMenu("Debug_generation")]
    public void Generate()
    {
        // FASE 1 Sceglie un seed
        if (_terrain == null || _terrainData == null)
        {
            Initialization();
        }

        _terrainData.heightmapResolution = resolution;

        int currentSeed = randomSeed ? UnityEngine.Random.Range(-10000, 10000) : seed;

        // FASE 2 Genera l'heightmap usando il perlin noise multi-ottava
        float[,] heights = GenerateHeightmap(currentSeed);

        // FASE 3 Applica il falloff
        if (useFalloff)
        {
            float[,] falloff = GenerateFalloffMap(resolution);
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    heights[y, x] = Mathf.Clamp01(heights[y, x] - falloff[y, x]);
                }
            }
        }

        // FASE 4 Applica erosione
        if (applyErosion)
        {
            heights = ThermalErosion(heights);
        }

        // FASE 5 Scrive l'heightmap sul terrain data
        _terrainData.SetHeights(0, 0, heights);
    }

    private float[,] GenerateHeightmap(int currentSeed)
    {
        float[,] map = new float[resolution, resolution];

        // Generiamo offset unici per ogni ottava usando il seed
        System.Random perlinRng = new System.Random(currentSeed);
        Vector2[] octavesOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = perlinRng.Next(-10000, 10000) + offset.x;
            float offsetY = perlinRng.Next(-10000, 10000) + offset.y;
            octavesOffsets[i] = new Vector2(offsetX, offsetY);
        }

        // Calcoliamo il valore massimo teorico sommabile (per la normalizzazione)
        float maxPossible = 0f;
        float amplitude = 1f;
        for (int i = 0; i < octaves; i++)
        {
            maxPossible += amplitude;
            amplitude *= persistance;
        }

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                amplitude = 1f;
                float frequency = 1f;
                float noiseHeight = 0f;

                // Sommiamo il contributo di ogni ottava sull'altezza
                for (int o = 0; o < octaves; o++)
                {
                    float sampleX = (x / (float)resolution * scale + octavesOffsets[o].x) / scale * frequency;
                    float sampleY = (y / (float)resolution * scale + octavesOffsets[o].y) / scale * frequency;

                    // Perlin restituisce [0,1]. Portiamo il risultato a [-1,1]
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2f - 1f;

                    // Aggiungiamo il contributo pesato per ampiezza
                    noiseHeight += perlinValue * amplitude;

                    // prepariamo i valori per la prossima ottava
                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                float normalized = (noiseHeight + maxPossible) / (2f * maxPossible);
                normalized = Mathf.Clamp01(normalized);

                // Applichiamo la curva
                map[y, x] = heightCurve.Evaluate(normalized) * heightMultiplier;
            }
        }

        return map;
    }

    private float[,] GenerateFalloffMap(int resolution)
    {
        float[,] map = new float[resolution, resolution];
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float newX = x / (float)resolution * 2f - 1f;
                float newY = y / (float)resolution * 2f - 1f;

                // Distanza Chebyshev - Produce una forma quadrata
                float value = Mathf.Max(Mathf.Abs(newX), Mathf.Abs(newY));

                map[y, x] = EvaluateFalloff(value);
            }
        }
        return map;
    }
    /// <summary>
    /// Quando value = 0 allora non taglia nulla. Quando value = 1 taglia l'altezza
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private float EvaluateFalloff(float value)
    {
        return 
            Mathf.Pow(value, falloffStrenght) /
            (Mathf.Pow(value, falloffStrenght) + Mathf.Pow(falloffShift - falloffShift * value, falloffStrenght));
    }

    float[,] ThermalErosion(float[,] heights)
    {
        // Direzioni dei 4 vicini: sinistra, destra, su, giù
        int[] dx = { -1, 1, 0, 0 };
        int[] dy = { 0, 0, -1, 1 };

        for (int iter = 0; iter < erosionIteration; iter++)
        {
            // Evitiamo i bordi (indice 0 e size-1) per non uscire dall'array
            for (int y = 1; y < resolution - 1; y++)
            {
                for (int x = 1; x < resolution - 1; x++)
                {
                    float maxDiff = 0f;
                    int bestN = -1;

                    // Trova il vicino verso cui è più "in discesa"
                    for (int n = 0; n < 4; n++)
                    {
                        float diff = heights[y, x] - heights[y + dy[n], x + dx[n]];
                        if (diff > maxDiff) { maxDiff = diff; bestN = n; }
                    }

                    // Se il dislivello supera la soglia -> erodi
                    if (bestN >= 0 && maxDiff > erosionStrenght)
                    {
                        float transfer = erosionStrenght * 0.5f;
                        heights[y, x] -= transfer;
                        heights[y + dy[bestN], x + dx[bestN]] += transfer;
                    }
                }
            }
        }
        return heights;
    }


}
