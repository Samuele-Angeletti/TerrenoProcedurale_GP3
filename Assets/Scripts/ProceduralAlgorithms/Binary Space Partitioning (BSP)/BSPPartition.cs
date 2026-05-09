using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Rappresenta un nodo dell'albero del BSP (Binary Space Partitioning)
/// E' un rettangolo che a sua volta può essere suddiviso in 2 parti:
/// (sinistra/destra o sotto/sopra)
/// </summary>
public class BSPPartition
{
    #region DATI GEOMETRICI
    /// <summary>
    /// Coordinata X del bordo sinistro della partizione
    /// </summary>
    public int X { get; }

    /// <summary>
    /// Coordinata Y del bordo inferiore della partizione
    /// </summary>
    public int Y { get; }

    /// <summary>
    /// Lunghezza della partizione in celle
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Altezza della partizione in celle
    /// </summary>
    public int Height { get; }
    #endregion

    #region STRUTTURA AD ALBERO
    /// <summary>
    /// Figlio della divisione che può essere sinistra o sopra
    /// </summary>
    public BSPPartition First { get; private set; }
    /// <summary>
    /// Figlio della divisione che può essere destra o sotto
    /// </summary>
    public BSPPartition Second { get; private set; }
    #endregion

    #region STANZA

    // per scelta architetturale usiamo una tupla
    public (int x, int y, int width, int height)? Room { get; private set; }

    #endregion

    #region COSTRUTTORE
    public BSPPartition(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
    #endregion

    #region PUBLIC API

    #endregion
}
