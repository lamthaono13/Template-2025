using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Tray : MonoBehaviour, IInteractTray
{
    private int id;

    private BlockModel currentModel;

    private TrayManager trayManager;

    [SerializeField] private Transform container;

    private bool canGet = true;

    private GameManager gameManager;

    private List<BlockView> spawned = new List<BlockView>();

    private Vector2 offSetTray;

    private float scaleTray;
    private float scaleBoard;

    // Start is called before the first frame update
    void Start()
    {
        EventBus.AddListener<StartDragEvent>(OnStartDrag);
        EventBus.AddListener<ContinueDragEvent>(OnContinueDrag);
        EventBus.AddListener<EndDragEvent>(OnEndDrag);
        EventBus.AddListener<EventChangedGrid>(OnGridChange);

        scaleTray = GameHelper.OffSetScaleTray * GameHelper.ScaleBoard();

        scaleBoard = GameHelper.ScaleBoard();
    }

    void OnDestroy()
    {
        EventBus.RemoveListener<StartDragEvent>(OnStartDrag);
        EventBus.RemoveListener<ContinueDragEvent>(OnContinueDrag);
        EventBus.RemoveListener<EndDragEvent>(OnEndDrag);
        EventBus.RemoveListener<EventChangedGrid>(OnGridChange);
    }

    private void OnStartDrag(StartDragEvent startDragEvent)
    {
        if (startDragEvent.iInteractTray.GetId() != id)
        {
            return;
        }

        OnGetTray();
    }

    private void OnContinueDrag(ContinueDragEvent continueDragEvent)
    {
        if(continueDragEvent.iInteractTray.GetId() != id)
        {
            return;
        }

        container.transform.position = continueDragEvent.pos + continueDragEvent.offSet;
    }

    private void OnEndDrag(EndDragEvent endDragEvent)
    {
        if (endDragEvent.iInteractTray.GetId() != id)
        {
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector2 GetOffSetTray()
    {
        return offSetTray;
    }

    public void Init(int _id, TrayManager _trayManager, GameManager _gameManager)
    {
        id = _id;

        trayManager = _trayManager;

        gameManager = _gameManager;

        SetCanGet(true);
    }

    public int GetId()
    {
        return id;
    }

    public BlockModel GetCurrentModel()
    {
        return currentModel;
    }

    public GameObject GetContainer()
    {
        return container.gameObject;
    }

    public void Reload(BlockModel blockModel, DataGrid dataGrid = null)
    {
        currentModel = blockModel;

        if(blockModel != null)
        {
            var cellSize = GameHelper.DefaultCellSize;

            float startX = -(float)(blockModel.shape.GetWidth() * cellSize) / 2f + cellSize / 2f;
            float startY = -(float)(blockModel.shape.GetHeight() * cellSize)/ 2f + cellSize / 2f;

            foreach (var c in blockModel.shape.cells)
            {
                int x = c.x;
                int y = c.y;

                if (x < 0 || y < 0 || x >= GridController.Width || y >= GridController.Height)
                    continue;

                var view = GetFromPool();
                view.transform.SetParent(container);


                float px = startX + x * cellSize;
                float py = startY + y * cellSize;

                //view.transform.localPosition = gameManager.Grid.GridRenderer.GridToLocal(x, y);

                view.transform.localPosition = new Vector3(px, py, 0);
                view.SetColor(blockModel.color);

                view.SetAlpha(1f);

                view.SetOrderLayer(GameHelper.OrderInLayerBlockInTray);

                spawned.Add(view);
            }

            offSetTray = new Vector2(startX, startY);

            container.transform.localScale = Vector3.zero;

            container.transform.DOScale(Vector3.one * scaleTray, 0.25f).SetEase(DG.Tweening.Ease.OutBack);
        }
        else
        {                    
            foreach(var t in spawned)
            {
                Recycle(t.gameObject);
            }

            spawned.Clear();
        }

        SetState(dataGrid);
    }

    public void OnGetTray()
    {
        if (tweenReturnToTray != null)
        {
            tweenReturnToTray.Kill();
        }

        if (tweenReturnScaleToTray != null)
        {
            tweenReturnScaleToTray.Kill();
        }

        container.transform.localScale = Vector3.one * scaleBoard;

        foreach (var t in spawned)
        {
            t.SetOrderLayer(GameHelper.OrderInLayerBlockDrag);
        }
    }

    private Tween tweenReturnToTray;
    private Tween tweenReturnScaleToTray;

    public void ReturnToTray(bool hasTween)
    {
        if (hasTween)
        {
            if(tweenReturnToTray != null)
            {
                tweenReturnToTray.Kill();
            }

            if (tweenReturnScaleToTray != null)
            {
                tweenReturnScaleToTray.Kill();
            }

            tweenReturnScaleToTray = container.transform.DOScale(Vector3.one * scaleTray, 0.25f).SetEase(DG.Tweening.Ease.OutBack);

            tweenReturnToTray = container.DOLocalMove(Vector3.zero, 0.25f).SetEase(DG.Tweening.Ease.OutBack).OnComplete(() =>
            {
                tweenReturnToTray = null;

                foreach (var t in spawned)
                {
                    t.SetOrderLayer(GameHelper.OrderInLayerBlockInTray);
                }
            });
        }
        else
        {
            container.transform.localPosition = Vector3.zero;
        }


    }

    private void SetCanGet(bool _canGet)
    {
        canGet = _canGet;
    }

    private BlockView GetFromPool()
    {
        var go = gameManager.PoolManager.GetPool<BlockView>(container);

        go.transform.localScale = Vector3.one;
        return go;
    }

    private void Recycle(GameObject g)
    {
        gameManager.PoolManager.TakeToPool<BlockView>(g);
    }

    public bool CanGetTray()
    {
        return canGet && currentModel != null;
    }

    private void OnGridChange(EventChangedGrid eventChangedGrid)
    {
        //Debug.LogError("!111");

        SetState(eventChangedGrid.dataGrid);

        //Debug.LogError("!333");
    }

    public void SetState(DataGrid dataGrid)
    {
        if (GetCurrentModel() == null)
        {
            return;
        }

        bool canPlace = GameHelper.HasAnyValidPlacement(GetCurrentModel().shape, dataGrid);

        SetCanGet(canPlace);

        if (!canPlace)
        {
            if (spawned != null)
            {
                for (int i = 0; i < spawned.Count; i++)
                {
                    if (spawned[i].CheckIsBrick())
                    {
                        continue;
                    }

                    spawned[i].ActiveBrick(true);

                    spawned[i].SetAlphaBrick(0);

                    int id = i;

                    DOTween.To((x) =>
                    {
                        spawned[id].SetAlphaBrick(x);

                    }, 0f, 1f, 0.5f);
                }
            }
        }
        else
        {
            for (int i = 0; i < spawned.Count; i++)
            {
                spawned[i].ActiveBrick(false);

                spawned[i].SetAlphaBrick(1);
            }
        }
    }
}

public interface IInteractTray
{
    public int GetId();

    public GameObject GetContainer();

    public void OnGetTray();

    public void ReturnToTray(bool hasTween);

    public bool CanGetTray();

    public BlockModel GetCurrentModel();

    public Vector2 GetOffSetTray();
}