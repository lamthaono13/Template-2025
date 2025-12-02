using System.Collections.Generic;
using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    [SerializeField] private GameManager GameManager;

    [SerializeField] private Transform root;

    // set by BoardScaler at runtime
    //[HideInInspector]
    private float cellSize = 1.12f;

    private BlockView[,] placed;

    private int width;
    private int height;

    private void Start()
    {
        EventBus.AddListener<EventClear>(OnClear);
        EventBus.AddListener<EventChangedGrid>(OnGridChange);
    }

    public void Init(int _width, int _height)
    {
        width = _width;
        height = _height;

        cellSize = GameHelper.DefaultCellSize;

        placed = new BlockView[width, height];
    }

    private BlockView GetFromPool()
    {
        var go = GameManager.PoolManager.GetPool<BlockView>(root);

        go.SetOrderLayer(GameHelper.OrderInLayerBlockOnBoard);

        go.SetAlpha(1f);

        go.transform.localScale = Vector3.one;

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
                        view.transform.localPosition = GameHelper.GridToLocal(x, y, cellSize, width, height);

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

    private void OnClear(EventClear eventClear)
    {

    }

    private void OnGridChange(EventChangedGrid eventChangedGrid)
    {
        ApplySnapshot(eventChangedGrid.dataGrid.Cells);
    }

    private void OnDestroy()
    {
        EventBus.RemoveListener<EventClear>(OnClear);
        EventBus.RemoveListener<EventChangedGrid>(OnGridChange);
    }

    //public Rect GetBoardWorldRect()
    //{
    //    if (root == null) return new Rect();

    //    Vector3 worldCenter = root.transform.position;
    //    float boardWorldWidth = cellSize * width * root.lossyScale.x;
    //    float boardWorldHeight = cellSize * height * root.lossyScale.y;

    //    float left = worldCenter.x - boardWorldWidth / 2f;
    //    float bottom = worldCenter.y - boardWorldHeight / 2f;

    //    return new Rect(left, bottom, boardWorldWidth, boardWorldHeight);
    //}
}