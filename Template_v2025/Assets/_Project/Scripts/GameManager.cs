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
}