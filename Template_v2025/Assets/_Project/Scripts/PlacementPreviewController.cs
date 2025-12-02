using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;

public class PlacementPreviewController : MonoBehaviour
{
    [SerializeField] private Transform previewRoot;

    [SerializeField] private GameManager gameManager;

    private List<BlockView> spawned = new List<BlockView>();

    private float cellSize = 1.12f;

    private int width;
    private int height;

    public Transform GetPreviewRoot()
    {
        return previewRoot;
    }

    public void Init(int _width, int _height)
    {
        width = _width;
        height = _height;

        if(spawned != null)
        {
            foreach (var item in spawned)
            {
                RecyclePreview(item);
            }
        }
        else
        {
            spawned = new List<BlockView>();
        }

        cellSize = GameHelper.DefaultCellSize;
    }

    private BlockView GetPreview()
    {
        var go = gameManager.PoolManager.GetPool<BlockView>(previewRoot);
        go.SetOrderLayer(GameHelper.OrderInLayerBlockPreview);
        go.transform.localScale = Vector3.one;
        return go;
    }

    private void RecyclePreview(BlockView g)
    {
        gameManager.PoolManager.TakeToPool<BlockView>(g.gameObject);
    }

    public void ShowPreview(ShapeData shape, DataClear dataClear, int ox, int oy, BlockColor color)
    {
        ClearPreview();

        //bool a = false;

        foreach (var c in shape.cells)
        {
            int x = ox + c.x;
            int y = oy + c.y;

            //Debug.LogError($"Showing preview at {x},{y}");

            if (x < 0 || y < 0 || x >= GridController.Width || y >= GridController.Height)
                continue;

            var view = GetPreview();
            view.transform.SetParent(previewRoot, false);
            view.transform.localPosition = GameHelper.GridToLocal(x, y, cellSize, width, height);
            view.SetColor(color);
            view.SetAlpha(0.6f);
            spawned.Add(view);
        }



        List<axis> clearCheck = new List<axis>();

        for (int i = 0; i < dataClear.ClearRows.Length; i++)
        {
            int ry = dataClear.ClearRows[i];
            for (int x = 0; x < GridController.Width; x++)
            {
                if(clearCheck.Exists(ac => ac.x == x && ac.y == ry))
                {
                    continue;
                }

                var view = GetPreview();
                view.transform.SetParent(previewRoot, false);
                view.transform.localPosition = GameHelper.GridToLocal(x, ry, cellSize, width, height);
                view.SetColor(color);
                view.SetAlpha(1);
                view.ActiveShine(true);
                spawned.Add(view);

                clearCheck.Add(new axis { x = x, y = ry });
            }
        }

        for (int i = 0; i < dataClear.ClearCols.Length; i++)
        {
            int cx = dataClear.ClearCols[i];
            for (int y = 0; y < GridController.Height; y++)
            {
                if (clearCheck.Exists(ac => ac.x == cx && ac.y == y))
                {
                    continue;
                }

                var view = GetPreview();
                view.transform.SetParent(previewRoot, false);
                view.transform.localPosition = GameHelper.GridToLocal(cx, y, cellSize, width, height);
                view.SetColor(color);
                view.SetAlpha(1);
                view.ActiveShine(true);
                spawned.Add(view);

                clearCheck.Add(new axis { x = cx, y = y });
            }
        }
    }

    public void ClearPreview()
    {
        foreach (var g in spawned)
        {
            if (g != null) RecyclePreview(g);
        }
        spawned.Clear();
    }
}

public struct axis
{
    public int x;
    public int y;
}
