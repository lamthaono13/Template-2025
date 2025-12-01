using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GridController grid;
    [SerializeField] private BlockSpawner spawner;
    [SerializeField] private TrayManager trayManager;
    [SerializeField] private PlacementPreviewController preview;
    [SerializeField] private PoolManager poolManager;
    [SerializeField] private WorldDragController worldDragController;
    [SerializeField] private GameEventManager gameEventManager;

    public PoolManager PoolManager => poolManager;
    public GridController Grid => grid;
    public BlockSpawner Spawner => spawner;

    public PlacementPreviewController Preview => preview;

    public TrayManager TrayManager => trayManager;

    private void Start()
    {
        // init

        poolManager.Init();

        grid.Init();

        spawner.Init();

        trayManager.Init();

        preview.Init();

        worldDragController.Init();

        gameEventManager.Init();

        // events


        //grid.OnCleared += OnCleared;
    }

    private void OnDestroy()
    {
        //grid.OnGridChanged -= OnGridChanged;
        //grid.OnCleared -= OnCleared;
    }

    private void OnGridChanged()
    {
        //var snap = grid.GetCellsSnapshot();
        //if (gridRenderer != null) gridRenderer.ApplySnapshot(snap);
    }

    private void OnCleared(int count)
    {
        Debug.Log($"Cleared {count} lines");
    }

    public BlockModel GetCurrentModelAt(int idx)
    {
        if (trayManager == null) return null;
        return trayManager.GetModelAt(idx);
    }

    public void TryPlaceFromUI(int idx, int ox, int oy)
    {
        var model = GetCurrentModelAt(idx);
        if (model == null) return;

        if (grid.CanPlaceShape(model.shape, ox, oy))
        {
            grid.PlaceShape(model.shape, model.color, ox, oy);
        }
        else
        {
            Debug.Log("GameManager: Cannot place shape at provided grid coords.");
        }
    }
}