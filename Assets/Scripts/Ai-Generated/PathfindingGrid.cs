using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Componente centrale del pathfinding.
/// - Si aggancia a un <see cref="GenerationResult"/> tramite <see cref="Initialize"/>.
/// - Espone <see cref="FindPath"/> per calcolare percorsi A*.
/// - Disegna Gizmos per visualizzare le celle calpestabili (solo in Editor).
/// 
/// Calcolo pensato per l'Edit Mode: chiama <see cref="Initialize"/> e poi
/// <see cref="FindPath"/> quando la mappa è pronta, non a ogni frame.
/// </summary>
public class PathfindingGrid : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Impostazioni Inspector
    // -------------------------------------------------------------------------

    [Header("Visualizzazione Gizmos")]
    [SerializeField] private Color _walkableColor  = new Color(0f, 1f, 0f, 0.15f);
    [SerializeField] private Color _walkableOutline = new Color(0f, 0.8f, 0f, 0.5f);
    [SerializeField] private float _cellSize        = 1f;
    [SerializeField] private bool  _drawGizmos      = true;

    // -------------------------------------------------------------------------
    // Stato interno
    // -------------------------------------------------------------------------

    private bool[,]          _walkable;
    private int              _width;
    private int              _height;
    private AStarPathfinder  _pathfinder;

    public bool IsInitialized => _pathfinder != null;

    // -------------------------------------------------------------------------
    // API pubblica
    // -------------------------------------------------------------------------

    /// <summary>
    /// Inizializza il grid a partire da un <see cref="GenerationResult"/>.
    /// Deve essere chiamato ogni volta che la mappa viene (ri)generata.
    /// </summary>
    public void Initialize(GenerationResult result)
    {
        _width    = result.Width;
        _height   = result.Height;
        _walkable = BuildWalkableMap(result);
        _pathfinder = new AStarPathfinder(_walkable);
    }

    /// <summary>
    /// Calcola il percorso A* tra due posizioni world-space.
    /// Restituisce la lista di posizioni world-space oppure null se non raggiungibile.
    /// </summary>
    public List<Vector3> FindPath(Vector3 worldStart, Vector3 worldEnd)
    {
        if (!IsInitialized)
        {
            Debug.LogWarning("[PathfindingGrid] Non inizializzato. Chiama Initialize() prima.");
            return null;
        }

        GridNode startNode = WorldToGrid(worldStart);
        GridNode endNode   = WorldToGrid(worldEnd);

        List<GridNode> nodePath = _pathfinder.FindPath(startNode, endNode);
        if (nodePath == null) return null;

        var worldPath = new List<Vector3>(nodePath.Count);
        foreach (var node in nodePath)
            worldPath.Add(GridToWorld(node));

        return worldPath;
    }

    /// <summary>
    /// Converte una posizione world-space nella cella griglia corrispondente.
    /// </summary>
    public GridNode WorldToGrid(Vector3 worldPos)
    {
        Vector3 local = worldPos - transform.position;
        int x = Mathf.RoundToInt(local.x / _cellSize);
        int y = Mathf.RoundToInt(local.z / _cellSize); // z perché il piano è XZ
        return new GridNode(x, y);
    }

    /// <summary>
    /// Converte una cella griglia nella posizione world-space del suo centro.
    /// </summary>
    public Vector3 GridToWorld(GridNode node)
    {
        return transform.position + new Vector3(node.X * _cellSize, 0f, node.Y * _cellSize);
    }

    /// <summary>
    /// Restituisce true se la cella è calpestabile.
    /// </summary>
    public bool IsWalkable(GridNode node) =>
        IsInitialized &&
        node.X >= 0 && node.X < _width &&
        node.Y >= 0 && node.Y < _height &&
        _walkable[node.X, node.Y];

    // -------------------------------------------------------------------------
    // Gizmos
    // -------------------------------------------------------------------------

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!_drawGizmos || !IsInitialized) return;

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (!_walkable[x, y]) continue;

                Vector3 center = GridToWorld(new GridNode(x, y));

                // Riempimento semitrasparente
                Gizmos.color = _walkableColor;
                Gizmos.DrawCube(center, new Vector3(_cellSize * 0.95f, 0.01f, _cellSize * 0.95f));

                // Bordo
                Gizmos.color = _walkableOutline;
                Gizmos.DrawWireCube(center, new Vector3(_cellSize, 0.01f, _cellSize));
            }
        }
    }
#endif

    // -------------------------------------------------------------------------
    // Privati
    // -------------------------------------------------------------------------

    private static bool[,] BuildWalkableMap(GenerationResult result)
    {
        var walkable = new bool[result.Width, result.Height];
        result.ForEachCell((x, y, value) => walkable[x, y] = (value == 0));
        return walkable;
    }
}
