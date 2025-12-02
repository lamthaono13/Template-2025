using System.Collections.Generic;
using System.ComponentModel;
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

        for(int i = 0; i < dataClear.ClearRows.Length; i++)
        {
            int ry = dataClear.ClearRows[i];
            for (int x = 0; x < GridController.Width; x++)
            {
                var view = GetPreview();
                view.transform.SetParent(previewRoot, false);
                view.transform.localPosition = GameHelper.GridToLocal(x, ry, cellSize, width, height);
                view.SetColor(color);
                view.SetAlpha(1);
                view.ActiveShine(true);
                spawned.Add(view);
            }
        }

        for (int i = 0; i < dataClear.ClearCols.Length; i++)
        {
            int cx = dataClear.ClearCols[i];
            for (int y = 0; y < GridController.Height; y++)
            {
                var view = GetPreview();
                view.transform.SetParent(previewRoot, false);
                view.transform.localPosition = GameHelper.GridToLocal(cx, y, cellSize, width, height);
                view.SetColor(color);
                view.SetAlpha(1);
                view.ActiveShine(true);
                spawned.Add(view);
            }
        }
    }

    public void ClearPreview()
    {
        foreach (var g in spawned)
        {
            if (g != null) gameManager.PoolManager.TakeToPool<BlockView>(g.gameObject);
        }
        spawned.Clear();
    }
}
