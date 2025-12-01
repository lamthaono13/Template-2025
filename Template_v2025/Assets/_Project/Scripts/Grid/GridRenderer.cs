using System.Collections.Generic;
using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    public GameManager GameManager;

    public Transform root;

    // set by BoardScaler at runtime
    //[HideInInspector]
    public float cellSize = 1f;


    public float blockLocalScale = 1f;

    private BlockView[,] placed = new BlockView[GridController.Width, GridController.Height];

    private int width;
    private int height;

    private void Start()
    {

    }

    public void Init(int _width, int _height)
    {
        width = _width;
        height = _height;
    }

    private BlockView GetFromPool()
    {
        var go = GameManager.PoolManager.GetPool<BlockView>(root);

        go.SetOrderLayer(0);

        go.SetAlpha(1f);

        go.transform.localScale = Vector3.one * blockLocalScale;
        return go;
    }

    private void Recycle(GameObject g)
    {
        GameManager.PoolManager.TakeToPool<BlockView>(g);
    }

    public void ClearAll()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (placed[x, y] != null)
                {
                    Recycle(placed[x, y].gameObject);
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
                        Recycle(cur.gameObject);
                        placed[x, y] = null;
                    }
                }
                else
                {
                    if (cur == null)
                    {
                        var view = GetFromPool();
                        view.transform.localPosition = GridToLocal(x, y);

                        view.SetColor(cell.color);
                        view.SetAlpha(1f);
                        placed[x, y] = view;
                    }
                    else
                    {
                        var view = cur;
                        view.SetColor(cell.color);
                        view.SetAlpha(1f);
                    }
                }
            }
        }
    }

    public Vector3 GridToLocal(int x, int y)
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
        return true;
    }

    public Rect GetBoardWorldRect()
    {
        if (root == null) return new Rect();

        Vector3 worldCenter = root.transform.position;
        float boardWorldWidth = cellSize * width * root.lossyScale.x;
        float boardWorldHeight = cellSize * height * root.lossyScale.y;

        float left = worldCenter.x - boardWorldWidth / 2f;
        float bottom = worldCenter.y - boardWorldHeight / 2f;

        return new Rect(left, bottom, boardWorldWidth, boardWorldHeight);
    }
}
