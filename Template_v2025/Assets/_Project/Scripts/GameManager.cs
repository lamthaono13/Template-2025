using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GridController grid;
    [SerializeField] private TrayManager trayManager;
    [SerializeField] private PoolManager poolManager;
    [SerializeField] private WorldDragController worldDragController;

    public PoolManager PoolManager => poolManager;
    //public GridController Grid => grid;
    //public BlockSpawner Spawner => spawner;


    //public TrayManager TrayManager => trayManager;

    private void Start()
    {
        // init

        poolManager.Init();

        trayManager.Init();

        grid.Init();

        worldDragController.Init();
    }

    private void OnDestroy()
    {

    }
}