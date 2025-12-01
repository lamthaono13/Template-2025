using System.Collections.Generic;
using UnityEngine;

public class PlacementPreviewController : MonoBehaviour
{
    [SerializeField] private Transform previewRoot;

    [SerializeField] private GameManager gameManager;

    private GridRenderer gridRenderer;

    private List<BlockView> spawned = new List<BlockView>();

    private void Start()
    {

    }

    public void Init()
    {
        gridRenderer = gameManager.Grid.GridRenderer;
    }

    private BlockView GetPreview()
    {
        var go = gameManager.PoolManager.GetPool<BlockView>(previewRoot);
        go.SetOrderLayer(0);
        go.transform.localScale = Vector3.one * gridRenderer.blockLocalScale;
        return go;
    }

    private void RecyclePreview(BlockView g)
    {

    }

    public void ShowPreview(ShapeData shape, int ox, int oy, bool canPlace, BlockColor color)
    {
        ClearPreview();

        if (gridRenderer == null)
        {
            Debug.LogWarning("PlacementPreviewController: gridRenderer not assigned.");
            return;
        }

        foreach (var c in shape.cells)
        {
            int x = ox + c.x;
            int y = oy + c.y;

            if (x < 0 || y < 0 || x >= GridController.Width || y >= GridController.Height)
                continue;

            var view = GetPreview();
            view.transform.SetParent(gridRenderer.root, false);
            view.transform.localPosition = gridRenderer.GridToLocal(x, y);
            view.SetColor(color);
            view.SetAlpha(canPlace ? 0.6f : 0.35f);
            spawned.Add(view);
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
