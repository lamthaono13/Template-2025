using DG.Tweening;
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

    [SerializeField] public GameObject objBoard;
    [SerializeField] public SpriteRenderer sr;

    private void Start()
    {
        EventBus.AddListener<EventClear>(OnClear);
        EventBus.AddListener<EventChangedGrid>(OnGridChange);

        objBoard.gameObject.transform.localScale = GameHelper.ScaleBoard() * Vector3.one;

        ScaleToScreen();
    }

    void ScaleToScreen()
    {
        if (!sr) return;

        // Kích thước sprite ban đầu (world)
        float spriteW = sr.bounds.size.x;
        float spriteH = sr.bounds.size.y;

        // Kích thước màn hình theo camera (world units)
        float worldH = Camera.main.orthographicSize * 2f;
        float worldW = worldH * Screen.width / Screen.height;

        // Tính scale cần thiết
        float scaleX = worldW / spriteW;
        float scaleY = worldH / spriteH;

        // Scale để BG phủ hết màn hình
        float finalScale = Mathf.Max(scaleX, scaleY);

        sr.transform.localScale = new Vector3(finalScale, finalScale, 1f);
    }

    public void Init(int _width, int _height)
    {
        width = _width;
        height = _height;

        cellSize = GameHelper.DefaultCellSize;

        if(placed != null)
        {
            foreach (var item in placed)
            {
                if (item != null)
                {
                    Recycle(item.gameObject);
                }
            }
        }

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
        Dictionary<axis, BlockView> clearCheck = new Dictionary<axis, BlockView>();

        DataClear dataClear = eventClear.dataClear;

        for (int i = 0; i < dataClear.ClearRows.Length; i++)
        {
            int ry = dataClear.ClearRows[i];
            for (int x = 0; x < GridController.Width; x++)
            {
                if (clearCheck.ContainsKey(new axis() { x = x, y = ry }))
                {
                    continue;
                }

                var view = GetFromPool();
                view.transform.localPosition = GameHelper.GridToLocal(x, ry, cellSize, width, height);

                view.SetColor(dataClear.blockColor);
                view.SetAlpha(1f);

                clearCheck.Add(new axis { x = x, y = ry }, view);
            }
        }

        for (int i = 0; i < dataClear.ClearCols.Length; i++)
        {
            int cx = dataClear.ClearCols[i];
            for (int y = 0; y < GridController.Height; y++)
            {
                if (clearCheck.ContainsKey(new axis() { x = cx, y = y }))
                {
                    continue;
                }

                var view = GetFromPool();
                view.transform.localPosition = GameHelper.GridToLocal(cx, y, cellSize, width, height);

                view.SetColor(dataClear.blockColor);
                view.SetAlpha(1f);

                clearCheck.Add(new axis { x = cx, y = y }, view);
            }
        }


        foreach(var view in clearCheck.Values)
        {
            // add shine effect

            view.ActiveShine(true);
            view.PlayVfxClear();

            DOTween.To((x) => 
            {
                view.SetAlpha(x);
                view.SetAlphaShine(x);
            }, 1.0f, 0.0f, 0.7f).OnComplete(() =>
            {
                Recycle(view.gameObject);
            });
        }
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