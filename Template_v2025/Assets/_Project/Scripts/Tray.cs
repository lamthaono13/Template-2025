using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Tray : MonoBehaviour
{
    private int id;

    private BlockModel currentModel;

    private TrayManager trayManager;

    [SerializeField] private Transform container;

    private bool canGet = true;

    private GameManager gameManager;

    private List<BlockView> spawned = new List<BlockView>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Init(int _id, BlockModel blockModel, TrayManager _trayManager, GameManager _gameManager)
    {
        id = _id;

        trayManager = _trayManager;

        gameManager = _gameManager;

        Reload(blockModel);
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

    public void Reload(BlockModel blockModel)
    {
        currentModel = blockModel;

        if(blockModel != null)
        {
            foreach (var c in blockModel.shape.cells)
            {
                int x = c.x;
                int y = c.y;

                if (x < 0 || y < 0 || x >= GridController.Width || y >= GridController.Height)
                    continue;

                var view = GetFromPool();
                view.transform.SetParent(container);

                var cellSize = gameManager.Grid.GridRenderer.cellSize;

                float startX = -blockModel.shape.GetWidth() / 2f + cellSize / 2f;
                float startY = -blockModel.shape.GetHeight() / 2f + cellSize / 2f;

                float px = startX + x * cellSize;
                float py = startY + y * cellSize;

                //view.transform.localPosition = gameManager.Grid.GridRenderer.GridToLocal(x, y);
                view.transform.localPosition = new Vector3(px, py, 0);
                view.SetColor(blockModel.color);

                view.SetAlpha(1f);

                view.SetOrderLayer(100);

                spawned.Add(view);
            }
        }
        else
        {                    
            foreach(var t in spawned)
            {
                Recycle(t.gameObject);
            }

            spawned.Clear();
        }
    }

    public void ReturnToTray(bool hasTween)
    {
        if (hasTween)
        {
            SetCanGet(false);

            container.DOLocalMove(Vector3.zero, 0.25f).SetEase(DG.Tweening.Ease.OutBack).OnComplete(() =>
            {
                SetCanGet(true);
            });
        }
        else
        {
            container.transform.localPosition = Vector3.zero;
        }


    }

    public void SetCanGet(bool _canGet)
    {
        canGet = _canGet;
    }

    public bool CanGet()
    {
        return canGet && currentModel != null;
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
}
