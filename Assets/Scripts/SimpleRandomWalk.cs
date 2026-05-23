using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRandomWalk : MonoBehaviour
{
    [Header("Simple Walker Settings")]
    [SerializeField] int gridDimension;
    [SerializeField] int attempts;

    [SerializeField] AlgorithmDirector algoDirector;
    [SerializeField] MeshProceduralGenerator meshGenerator;
    [SerializeField] PathfindingGrid pathfindingGrid;
    bool[,] grid;

    GenerationResult generationResult;

    [ContextMenu("Debug_ComplexWalker")]
    public void GenerateComplexDebug()
    {
        generationResult = algoDirector.Generate(AlgorithmDirector.AlgorithmType.RandomWalk);
        meshGenerator.Generate(generationResult);
    }

    [ContextMenu("Debug_BSP")]
    public void GenerateBSPDebug()
    {
        generationResult = algoDirector.Generate(AlgorithmDirector.AlgorithmType.BSP);
        meshGenerator.Generate(generationResult);
    }
    [ContextMenu("Debug_CA")]
    public void GenerateCADebug()
    {
        generationResult = algoDirector.Generate(AlgorithmDirector.AlgorithmType.CellularAutomata);
        meshGenerator.Generate(generationResult);
        pathfindingGrid.Initialize(generationResult);
    }

    [ContextMenu("Debug_SimpleWalker")]
    public void GenerateDebug()
    {
        StartCoroutine(Generate());
    }
    public IEnumerator Generate()
    {
        grid = new bool[gridDimension, gridDimension];

        var startPointX = Random.Range(0, gridDimension);
        var startPointY = Random.Range(0, gridDimension);

        grid[startPointY, startPointX] = true;

        Vector2Int currentPosition = new(startPointX, startPointY);

        int currentAttempt = 0;

        do
        {
            if (Random.Range(0, 2) == 0)
            {
                // su / gi 
                if (Random.Range(0, 2) == 0)
                {
                    // su
                    currentPosition.y += 1;
                }
                else
                {
                    // gi 
                    currentPosition.y -= 1;
                }
            }
            else
            {
                // dx / sx
                if (Random.Range(0, 2) == 0)
                {
                    // dx
                    currentPosition.x += 1;
                }
                else
                {
                    // sx
                    currentPosition.x -= 1;
                }
            }

            if (currentPosition.y < 0 || currentPosition.y >= gridDimension || currentPosition.x < 0 || currentPosition.x >= gridDimension
                || grid[currentPosition.y, currentPosition.x])
            {
                currentAttempt++;
                // cerca un altro punto dal quale partire
                List<Vector2Int> points = new List<Vector2Int>();
                for (int y = 0; y < gridDimension; y++)
                {
                    for (int x = 0; x < gridDimension; x++)
                    {
                        if (grid[y, x])
                        {
                            points.Add(new Vector2Int(x, y));
                        }
                    }
                }

                currentPosition = points[Random.Range(0, points.Count)];
            }
            else
            {
                grid[currentPosition.y, currentPosition.x] = true;
            }

            yield return new WaitForSeconds(0.2f);
        }
        while (currentAttempt <= attempts);
    }

    private void OnDrawGizmos()
    {
        // if (grid != null)
        // {
        //     for (int y = 0; y < gridDimension; y++)
        //     {
        //         for (int x = 0; x < gridDimension; x++)
        //         {
        //             if (grid[y, x])
        //             {
        //                 Gizmos.color = Color.yellow;
        //                 Gizmos.DrawCube(new Vector3(x, 0, y), Vector3.one);
        //             }
        //         }
        //     }
        // }

        // if (generationResult != null)
        // {
        //     generationResult.ForEachCell((x, y, value) =>
        //     {
        //         Gizmos.color = value == 0 ? Color.green : Color.gray;
        //         Gizmos.DrawCube(new Vector3(x, 0, y), Vector3.one);
        //     });
        // }
    }
}
