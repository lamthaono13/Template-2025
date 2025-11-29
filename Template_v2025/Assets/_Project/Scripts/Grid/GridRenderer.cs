using System.Collections.Generic;
using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    [Tooltip("Prefab for a single block (must have SpriteRenderer + BlockView)")]
    public GameObject blockPrefab;

    [Tooltip("Parent transform where instantiated blocks will be placed. Should be child of Board.")]
    public Transform root;

    // set by BoardScaler at runtime
    //[HideInInspector]
    public float cellSize = 1f;

    // scale applied to block instances (computed by BoardScaler)
    //[HideInInspector]
    public float blockLocalScale = 1f;

    private Stack<GameObject> pool = new Stack<GameObject>();
    private GameObject[,] placed = new GameObject[GridController.Width, GridController.Height];

    private GameObject GetFromPool()
    {
        if (pool.Count > 0)
        {
            var g = pool.Pop();
            g.SetActive(true);
            return g;
        }

        var go = Instantiate(blockPrefab, root);
        go.transform.localScale = Vector3.one * blockLocalScale;
        return go;
    }

    private void Recycle(GameObject g)
    {
        g.SetActive(false);
        pool.Push(g);
    }

    public void ClearAll()
    {
        for (int x = 0; x < GridController.Width; x++)
        {
            for (int y = 0; y < GridController.Height; y++)
            {
                if (placed[x, y] != null)
                {
                    Recycle(placed[x, y]);
                    placed[x, y] = null;
                }
            }
        }
    }

    public void ApplySnapshot(BlockCell[,] snap)
    {
        for (int x = 0; x < GridController.Width; x++)
        {
            for (int y = 0; y < GridController.Height; y++)
            {
                var cell = snap[x, y];
                var cur = placed[x, y];

                if (cell == null)
                {
                    if (cur != null)
                    {
                        Recycle(cur);
                        placed[x, y] = null;
                    }
                }
                else
                {
                    if (cur == null)
                    {
                        var go = GetFromPool();
                        go.transform.localPosition = GridToLocal(x, y);
                        var view = go.GetComponent<BlockView>();
                        view.SetColor(cell.color);
                        view.SetAlpha(1f);
                        placed[x, y] = go;
                    }
                    else
                    {
                        var view = cur.GetComponent<BlockView>();
                        view.SetColor(cell.color);
                        view.SetAlpha(1f);
                    }
                }
            }
        }
    }

    // convert grid coords to local position relative to root
    public Vector3 GridToLocal(int x, int y)
    {
    float boardWidth = cellSize * GridController.Width;
    float boardHeight = cellSize * GridController.Height;

    // center-based
    float startX = -boardWidth / 2f + cellSize / 2f;
    float startY = -boardHeight / 2f + cellSize / 2f;

    float px = startX + x * cellSize;
    float py = startY + y * cellSize;

    return new Vector3(px, py, 0f);
    }

    /// <summary>
    /// Convert a world position to grid indices (ox,oy). Returns true if inside board bounds.
    /// </summary>
    public bool WorldToGrid(Vector3 worldPos, out int outX, out int outY)
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

        float boardWidth = cellSize * GridController.Width;
        float boardHeight = cellSize * GridController.Height;

        float startX = -boardWidth / 2f + cellSize / 2f;
        float startY = -boardHeight / 2f + cellSize / 2f;

        float fx = (local.x - startX) / cellSize;
        float fy = (local.y - startY) / cellSize;

        // Use RoundToInt so we snap to nearest cell center (consistent with GridToLocal centers)
        int ix = Mathf.RoundToInt(fx);
        int iy = Mathf.RoundToInt(fy);

        if (ix < 0 || ix >= GridController.Width || iy < 0 || iy >= GridController.Height)
            return false;

        outX = ix;
        outY = iy;
        return true;
    }

    public Rect GetBoardWorldRect()
    {
        if (root == null) return new Rect();

        Vector3 worldCenter = root.transform.position;
        float boardWorldWidth = cellSize * GridController.Width * root.lossyScale.x;
        float boardWorldHeight = cellSize * GridController.Height * root.lossyScale.y;

        float left = worldCenter.x - boardWorldWidth / 2f;
        float bottom = worldCenter.y - boardWorldHeight / 2f;

        return new Rect(left, bottom, boardWorldWidth, boardWorldHeight);
    }
}
