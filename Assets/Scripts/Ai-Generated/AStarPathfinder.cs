using System.Collections.Generic;

/// <summary>
/// Algoritmo A* puro. Nessuna dipendenza da Unity.
/// Opera su un grid booleano: true = calpestabile, false = muro.
/// </summary>
public class AStarPathfinder
{
    private readonly bool[,] _walkable;
    private readonly int _width;
    private readonly int _height;

    public AStarPathfinder(bool[,] walkable)
    {
        _walkable = walkable;
        _width    = walkable.GetLength(0);
        _height   = walkable.GetLength(1);
    }

    /// <summary>
    /// Calcola il percorso più breve da start a end.
    /// Restituisce la lista di nodi (inclusi start ed end) oppure null se non raggiungibile.
    /// </summary>
    public List<GridNode> FindPath(GridNode start, GridNode end)
    {
        if (!IsWalkable(start) || !IsWalkable(end))
            return null;

        // gScore[n]  = costo effettivo da start a n
        // fScore[n]  = gScore[n] + euristica(n, end)
        // parent[n]  = nodo precedente nel percorso ottimale
        var gScore = new Dictionary<GridNode, float> { [start] = 0f };
        var fScore = new Dictionary<GridNode, float> { [start] = Heuristic(start, end) };
        var parent = new Dictionary<GridNode, GridNode>();

        var openHeap  = new MinHeap<GridNode, float>();
        var inOpen    = new HashSet<GridNode>();
        var closedSet = new HashSet<GridNode>();

        openHeap.Push(start, fScore[start]);
        inOpen.Add(start);

        while (openHeap.Count > 0)
        {
            var current = openHeap.Pop();
            inOpen.Remove(current);

            if (current.Equals(end))
                return ReconstructPath(parent, current);

            closedSet.Add(current);

            foreach (var neighbour in GetNeighbours(current))
            {
                if (closedSet.Contains(neighbour) || !IsWalkable(neighbour))
                    continue;

                float tentativeG = gScore[current] + 1f; // costo uniforme

                float neighbourG = gScore.TryGetValue(neighbour, out var ng) ? ng : float.MaxValue;
                if (tentativeG >= neighbourG)
                    continue;

                parent[neighbour] = current;
                gScore[neighbour] = tentativeG;
                float f = tentativeG + Heuristic(neighbour, end);
                fScore[neighbour] = f;

                if (!inOpen.Contains(neighbour))
                {
                    openHeap.Push(neighbour, f);
                    inOpen.Add(neighbour);
                }
                else
                {
                    openHeap.UpdatePriority(neighbour, f);
                }
            }
        }

        return null; // Nessun percorso trovato
    }

    // -------------------------------------------------------------------------
    // Privati
    // -------------------------------------------------------------------------

    private bool IsWalkable(GridNode n) =>
        n.X >= 0 && n.X < _width &&
        n.Y >= 0 && n.Y < _height &&
        _walkable[n.X, n.Y];

    /// <summary>Vicini 4-direzionali.</summary>
    private static IEnumerable<GridNode> GetNeighbours(GridNode n)
    {
        yield return new GridNode(n.X + 1, n.Y);
        yield return new GridNode(n.X - 1, n.Y);
        yield return new GridNode(n.X,     n.Y + 1);
        yield return new GridNode(n.X,     n.Y - 1);
    }

    /// <summary>Manhattan distance — ottimale con vicini 4-direzionali.</summary>
    private static float Heuristic(GridNode a, GridNode b) =>
        System.Math.Abs(a.X - b.X) + System.Math.Abs(a.Y - b.Y);

    private static List<GridNode> ReconstructPath(Dictionary<GridNode, GridNode> parent, GridNode current)
    {
        var path = new List<GridNode>();
        while (parent.ContainsKey(current))
        {
            path.Add(current);
            current = parent[current];
        }
        path.Add(current); // aggiungi lo start
        path.Reverse();
        return path;
    }
}

// =============================================================================
// MinHeap<TItem, TPriority> — min-heap binario generico
// =============================================================================

/// <summary>
/// Min-heap binario con supporto all'aggiornamento della priorità in O(n).
/// Per grid di gioco la dimensione è contenuta, O(n) è accettabile.
/// </summary>
internal class MinHeap<TItem, TPriority> where TPriority : System.IComparable<TPriority>
{
    private readonly List<(TItem item, TPriority priority)> _heap = new();
    private readonly Dictionary<TItem, int> _indices = new();

    public int Count => _heap.Count;

    public void Push(TItem item, TPriority priority)
    {
        _heap.Add((item, priority));
        _indices[item] = _heap.Count - 1;
        BubbleUp(_heap.Count - 1);
    }

    public TItem Pop()
    {
        var top = _heap[0].item;
        _indices.Remove(top);
        var last = _heap[_heap.Count - 1];
        _heap[0] = last;
        _heap.RemoveAt(_heap.Count - 1);
        if (_heap.Count > 0)
        {
            _indices[last.item] = 0;
            SiftDown(0);
        }
        return top;
    }

    public void UpdatePriority(TItem item, TPriority priority)
    {
        if (!_indices.TryGetValue(item, out int i)) return;
        _heap[i] = (item, priority);
        BubbleUp(i);
        SiftDown(_indices.TryGetValue(item, out int j) ? j : i);
    }

    private void BubbleUp(int i)
    {
        while (i > 0)
        {
            int parent = (i - 1) / 2;
            if (_heap[parent].priority.CompareTo(_heap[i].priority) <= 0) break;
            Swap(i, parent);
            i = parent;
        }
    }

    private void SiftDown(int i)
    {
        int n = _heap.Count;
        while (true)
        {
            int left  = 2 * i + 1;
            int right = 2 * i + 2;
            int smallest = i;
            if (left  < n && _heap[left].priority.CompareTo(_heap[smallest].priority)  < 0) smallest = left;
            if (right < n && _heap[right].priority.CompareTo(_heap[smallest].priority) < 0) smallest = right;
            if (smallest == i) break;
            Swap(i, smallest);
            i = smallest;
        }
    }

    private void Swap(int a, int b)
    {
        (_heap[a], _heap[b]) = (_heap[b], _heap[a]);
        _indices[_heap[a].item] = a;
        _indices[_heap[b].item] = b;
    }
}
