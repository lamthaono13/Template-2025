using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine;
using System;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GridController grid;
    [SerializeField] private TrayManager trayManager;
    [SerializeField] private PoolManager poolManager;
    [SerializeField] private WorldDragController worldDragController;
    [SerializeField] private UIManager uIManager;

    private GameState gameState;

    public PoolManager PoolManager => poolManager;
    //public GridController Grid => grid;
    //public BlockSpawner Spawner => spawner;


    //public TrayManager TrayManager => trayManager;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    private void Start()
    {
        EventBus.AddListener<EventChangedGrid>(OnGridChange);
        EventBus.AddListener<EventReplay>(OnReplay);

        Init();
    }

    private void OnGridChange(EventChangedGrid grid)
    {
        BlockModel[] currentTrio = trayManager.GetCurrentDataTrays();

        DataGrid dataGrid = grid.dataGrid;

        bool hasValidPlacement = false;

        for (int i = 0; i < currentTrio.Length; i++)
        {
            BlockModel model = currentTrio[i];

            if(model == null)
            {
                continue;
            }

            if(GameHelper.HasAnyValidPlacement(model.shape, dataGrid))
            {
                hasValidPlacement = true;

                break;
            }
        }

        if (!hasValidPlacement)
        {
            gameState = GameState.Lose;

            StartCoroutine(WaitLose());
        }
    }

    IEnumerator WaitLose()
    {
        yield return new WaitForSeconds(0.7f);
        // cannot place this block anymore, game over
        DataEndGame dataEndGame = new DataEndGame()
        {
            level = 1,
            score = 0
        };
        EventBus.Raise(new EventEndGame(dataEndGame));
    }

    private void OnReplay(EventReplay eventReplay)
    {
        gameState = GameState.None;

        Init();
    }

    [Button]
    private void Init()
    {
        poolManager.Init();

        trayManager.Init();

        grid.Init();

        worldDragController.Init();

        uIManager.Init();
    }

    private void OnDestroy()
    {
        EventBus.RemoveListener<EventChangedGrid>(OnGridChange);
        EventBus.RemoveListener<EventReplay>(OnReplay);
    }
}

public class DataEndGame
{
    public int score;
    public int level;
}

public enum GameState
{
    None,
    Lose
}

public struct EventEndGame : IGameEvent
{
    public DataEndGame data;

    public EventEndGame(DataEndGame _data)
    {
        data = _data;
    }
}

public struct EventReplay: IGameEvent
{

}