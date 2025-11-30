using System.Collections.Generic;
using UnityEngine;

public class PlacementPreviewController : MonoBehaviour
{
    public GameObject blockPrefab;
    public Transform previewRoot;

    public GridRenderer gridRenderer;

    private List<GameObject> spawned = new List<GameObject>();
    private Stack<GameObject> pool = new Stack<GameObject>();

    private void Start()
    {
        for (int x = 0; x < GridController.Width; x++)
        {
            for (int y = 0; y < GridController.Height; y++)
            {
                var go = Instantiate(blockPrefab, previewRoot);
                go.transform.localScale = Vector3.one * gridRenderer.blockLocalScale;

                go.gameObject.SetActive(false);

                pool.Push(go);
            }
        }
    }

    private GameObject GetPreview()
    {
        if (pool.Count > 0)
        {
            var g = pool.Pop();
            g.SetActive(true);
            return g;
        }
        var go = Instantiate(blockPrefab, previewRoot);
        go.transform.localScale = Vector3.one * gridRenderer.blockLocalScale;
        return go;
    }

    private void RecyclePreview(GameObject g)
    {
        g.SetActive(false);
        pool.Push(g);
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

            var go = GetPreview();
            go.transform.SetParent(gridRenderer.root, false);
            go.transform.localPosition = gridRenderer.GridToLocal(x, y);
            var view = go.GetComponent<BlockView>();
            view.SetColor(color);
            view.SetAlpha(canPlace ? 0.6f : 0.35f);
            spawned.Add(go);
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
