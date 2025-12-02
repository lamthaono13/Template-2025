using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class GameHelper
{
    public static float DefaultCellSize = 1.18f;

    public static int OrderInLayerBlockOnBoard = 0;

    public static int OrderInLayerBlockPreview = 5;

    public static int OrderInLayerBlockInTray = 10;

    public static int OrderInLayerBlockDrag = 100;

    public static Vector3 GridToLocal(int x, int y, float cellSize, int width, int height)
    {
        float boardWidth = cellSize * width;
        float boardHeight = cellSize * height;

        // center-based
        float startX = -boardWidth / 2f + cellSize / 2f;
        float startY = -boardHeight / 2f + cellSize / 2f;

        float px = startX + x * cellSize;
        float py = startY + y * cellSize;

        return new Vector3(px, py, 0f);
    }


    public static bool WorldToGrid(Vector3 worldPos, Transform root, float cellSize, int width, int height, out int outX, out int outY)
    {
        outX = -1;
        outY = -1;

        if (root == null)
        {
            Debug.LogWarning("[GridRenderer] root not assigned");
            return false;
        }

        // convert world to local (root is child of Board)
        Vector3 local = root.InverseTransformPoint(worldPos);

        float boardWidth = cellSize * width;
        float boardHeight = cellSize * height;

        float startX = -boardWidth / 2f + cellSize / 2f;
        float startY = -boardHeight / 2f + cellSize / 2f;

        float fx = (local.x - startX) / cellSize;
        float fy = (local.y - startY) / cellSize;

        // Use RoundToInt so we snap to nearest cell center (consistent with GridToLocal centers)
        int ix = Mathf.RoundToInt(fx);
        int iy = Mathf.RoundToInt(fy);

        if (ix < 0 || ix >= width || iy < 0 || iy >= height)
            return false;

        outX = ix;
        outY = iy;

        //Debug.LogError(outX + "," + outY);

        return true;
    }

    public static bool HasAnyValidPlacement(ShapeData shape, DataGrid dataGrid)
    {
        for (int oy = 0; oy < dataGrid.Height; oy++)
            for (int ox = 0; ox < dataGrid.Width; ox++)
                if (CanPlaceShape(shape, dataGrid, ox, oy)) return true;
        return false;
    }

    public static bool CanPlaceShape(ShapeData shape, DataGrid dataGrid, int ox, int oy)
    {
        foreach (var c in shape.cells)
        {
            int x = ox + c.x;
            int y = oy + c.y;

            if (!IsInside(x, y, dataGrid.Width, dataGrid.Height))
                return false;

            if (dataGrid.Cells[x, y] != null)
                return false;
        }

        return true;
    }

    public static bool IsInside(int x, int y, int Width, int Height)
    {
        return x >= 0 && y >= 0 && x < Width && y < Height;
    }
}