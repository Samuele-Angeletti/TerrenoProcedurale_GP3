using System.Collections.Generic;
using UnityEngine;

public class SimpleRandomWalk : MonoBehaviour
{
    [SerializeField] int gridDimension;
    [SerializeField] int attempts;

    bool[,] grid;

    private void Start()
    {
        Generate();
    }

    [ContextMenu("Debug_generation")]
    public void Generate()
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
                // su / giù
                if (Random.Range(0, 2) == 0)
                {
                    // su
                    currentPosition.y += 1;
                }
                else
                {
                    // giù
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
        }
        while (currentAttempt <= attempts);
    }

    private void OnDrawGizmos()
    {
        if (grid == null)
            return;

        for (int y = 0; y < gridDimension; y++)
        {
            for (int x = 0; x < gridDimension; x++)
            {
                if (grid[y, x])
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawCube(new Vector3(x, 0, y), Vector3.one);
                }
            }
        }
    }
}
