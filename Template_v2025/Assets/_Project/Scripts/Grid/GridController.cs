using System;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour
{
    public const int Width = 8;
    public const int Height = 8;

    private BlockCell[,] cells = new BlockCell[Width, Height];

    public event Action<int> OnCleared;
    public event Action OnGridChanged;

    [SerializeField] private GridRenderer gridRenderer;

    public GridRenderer GridRenderer => gridRenderer;

    private void Awake()
    {

    }

    public void Init()
    {
        ClearGrid();

        gridRenderer.Init(Width, Height);

        OnGridChanged += () =>
        {
            var snap = GetCellsSnapshot();
            gridRenderer.ApplySnapshot(snap);
        };
    }

    public void ClearGrid()
    {
        cells = new BlockCell[Width, Height];
        OnGridChanged?.Invoke();
    }

    public bool IsInside(int x, int y)
    {
        return x >= 0 && y >= 0 && x < Width && y < Height;
    }

    public bool CanPlaceShape(ShapeData shape, int ox, int oy)
    {
        foreach (var c in shape.cells)
        {
            int x = ox + c.x;
            int y = oy + c.y;

            if (!IsInside(x, y))
                return false;

            if (cells[x, y] != null)
                return false;
        }

        return true;
    }

    public void PlaceShape(ShapeData shape, BlockColor color, int ox, int oy)
    {
        foreach (var c in shape.cells)
        {
            int x = ox + c.x;
            int y = oy + c.y;

            if (!IsInside(x, y))
            {
                Debug.LogError($"PlaceShape out of bounds: {x},{y}");
                continue;
            }

            cells[x, y] = new BlockCell
            {
                color = color
            };
        }

        int cleared = ClearFullLines();

        if (cleared > 0)
            OnCleared?.Invoke(cleared);

        OnGridChanged?.Invoke();
    }

    private int ClearFullLines()
    {
        List<int> rowsToClear = new List<int>();
        List<int> colsToClear = new List<int>();

        for (int y = 0; y < Height; y++)
        {
            bool full = true;
            for (int x = 0; x < Width; x++)
            {
                if (cells[x, y] == null) { full = false; break; }
            }
            if (full) rowsToClear.Add(y);
        }

        for (int x = 0; x < Width; x++)
        {
            bool full = true;
            for (int y = 0; y < Height; y++)
            {
                if (cells[x, y] == null) { full = false; break; }
            }
            if (full) colsToClear.Add(x);
        }

        // clear
        foreach (int y in rowsToClear)
            for (int x = 0; x < Width; x++)
                cells[x, y] = null;

        foreach (int x in colsToClear)
            for (int y = 0; y < Height; y++)
                cells[x, y] = null;

        return rowsToClear.Count + colsToClear.Count;
    }

    public bool HasAnyValidPlacement(ShapeData shape)
    {
        for (int oy = 0; oy < Height; oy++)
            for (int ox = 0; ox < Width; ox++)
                if (CanPlaceShape(shape, ox, oy)) return true;
        return false;
    }

    public bool AnyOfShapesPlaceable(IEnumerable<ShapeData> shapes)
    {
        foreach (var s in shapes) if (HasAnyValidPlacement(s)) return true;
        return false;
    }

    public BlockCell[,] GetCellsSnapshot()
    {
        var snap = new BlockCell[Width, Height];
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                snap[x, y] = cells[x, y] == null ? null : new BlockCell { color = cells[x, y].color };
        return snap;
    }
}

[System.Serializable]
public class BlockCell
{
    public BlockColor color;
}
