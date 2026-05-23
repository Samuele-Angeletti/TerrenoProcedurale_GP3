using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Script d'esempio: visualizza il percorso A* tra due Transform in Edit Mode.
///
/// Setup:
/// 1. Aggiungi questo componente allo stesso GameObject (o a uno figlio) di <see cref="PathfindingGrid"/>.
/// 2. Assegna <see cref="_startTransform"/> e <see cref="_endTransform"/> nell'Inspector.
/// 3. Premi il pulsante "Calcola Percorso" nell'Inspector (o modifica i Transform).
///
/// Il percorso viene ridisegnato automaticamente ogni volta che i Transform si spostano in Edit Mode
/// grazie a <see cref="OnDrawGizmos"/>.
/// </summary>
[RequireComponent(typeof(PathfindingGrid))]
public class PathfindingDebugger : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector
    // -------------------------------------------------------------------------

    [Header("Punti di percorso")]
    [SerializeField] private Transform _startTransform;
    [SerializeField] private Transform _endTransform;

    [Header("Visualizzazione")]
    [SerializeField] private Color _pathColor      = Color.yellow;
    [SerializeField] private Color _startColor     = Color.green;
    [SerializeField] private Color _endColor       = Color.red;
    [SerializeField] private float _nodeRadius     = 0.2f;
    [SerializeField] private float _lineThickness  = 3f;

    // -------------------------------------------------------------------------
    // Stato interno
    // -------------------------------------------------------------------------

    private List<Vector3>    _lastPath;
    private PathfindingGrid  _grid;

    // -------------------------------------------------------------------------
    // API pubblica
    // -------------------------------------------------------------------------

    /// <summary>
    /// Esegue il calcolo del percorso e memorizza il risultato per i Gizmos.
    /// Chiamato dal custom Editor tramite il pulsante Inspector oppure manualmente.
    /// </summary>
    public void ComputePath()
    {
        _grid = GetComponent<PathfindingGrid>();

        if (!_grid.IsInitialized)
        {
            Debug.LogWarning("[PathfindingDebugger] PathfindingGrid non inizializzato.");
            _lastPath = null;
            return;
        }

        if (_startTransform == null || _endTransform == null)
        {
            Debug.LogWarning("[PathfindingDebugger] Assegna Start e End Transform.");
            _lastPath = null;
            return;
        }

        _lastPath = _grid.FindPath(_startTransform.position, _endTransform.position);

        if (_lastPath == null)
            Debug.Log("[PathfindingDebugger] Nessun percorso trovato.");
        else
            Debug.Log($"[PathfindingDebugger] Percorso trovato: {_lastPath.Count} nodi.");
    }

    // -------------------------------------------------------------------------
    // Gizmos
    // -------------------------------------------------------------------------

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        DrawEndpoints();

        if (_lastPath == null || _lastPath.Count < 2) return;

        // Linea del percorso
        Handles.color = _pathColor;
        for (int i = 0; i < _lastPath.Count - 1; i++)
            Handles.DrawLine(_lastPath[i] + Vector3.up * 0.05f,
                             _lastPath[i + 1] + Vector3.up * 0.05f,
                             _lineThickness);

        // Nodi intermedi
        Gizmos.color = _pathColor;
        for (int i = 1; i < _lastPath.Count - 1; i++)
            Gizmos.DrawSphere(_lastPath[i] + Vector3.up * 0.05f, _nodeRadius * 0.6f);
    }

    private void DrawEndpoints()
    {
        if (_startTransform != null)
        {
            Gizmos.color = _startColor;
            Gizmos.DrawSphere(_startTransform.position, _nodeRadius);
            Handles.Label(_startTransform.position + Vector3.up * 0.5f, "START");
        }

        if (_endTransform != null)
        {
            Gizmos.color = _endColor;
            Gizmos.DrawSphere(_endTransform.position, _nodeRadius);
            Handles.Label(_endTransform.position + Vector3.up * 0.5f, "END");
        }
    }
#endif
}

// =============================================================================
// Custom Editor — pulsante "Calcola Percorso" nell'Inspector
// =============================================================================

#if UNITY_EDITOR
[CustomEditor(typeof(PathfindingDebugger))]
public class PathfindingDebuggerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();

        if (GUILayout.Button("Calcola Percorso", GUILayout.Height(30)))
        {
            var debugger = (PathfindingDebugger)target;
            debugger.ComputePath();
            SceneView.RepaintAll();
        }
    }
}
#endif
