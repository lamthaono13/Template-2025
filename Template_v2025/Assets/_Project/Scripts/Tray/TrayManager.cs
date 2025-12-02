using System;
using System.Collections.Generic;
using UnityEngine;


public class TrayManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Tray[] trays;
    [SerializeField] private GameManager gameManager;
    [Tooltip("Scale of tray shapes relative to board cell size")]
    public float visualScale = 0.6f;

    private BlockModel[] currentTrio = new BlockModel[0];



    [SerializeField] private BlockSpawner spawner;

    private void Awake()
    {
        // allocate arrays
        int slots = Mathf.Max(3, trays.Length);
    }

    private void Start()
    {
        EventBus.AddListener<EventTrayShapePlaced>(OnTrayShapePlaced);
        EventBus.AddListener<EventChangedGrid>(OnGridChange);
    }

    private void OnTrayShapePlaced(EventTrayShapePlaced placed)
    {
        RemoveSlotAndRefill(placed.iInteractTray);
    }

    private void OnDestroy()
    {
        EventBus.RemoveListener<EventTrayShapePlaced>(OnTrayShapePlaced);
        EventBus.RemoveListener<EventChangedGrid>(OnGridChange);
    }

    public void Init()
    {
        //if (spawner == null)
        //{
        //    spawner = gameManager.Spawner;
        //}

        spawner.Init();

        for (int i = 0; i < trays.Length; i++)
        {
            trays[i].Init(i, this, gameManager);

            trays[i].Reload(null);
        }
    }

    //public void GenerateInitial(DataGrid dataGrid)
    //{
    //    //if (spawner == null)
    //    //{
    //    //    Debug.LogWarning("[TrayManager] spawner not assigned");
    //    //    return;
    //    //}

    //    // generate trio using spawner
    //    currentTrio = spawner.GenerateThree(dataGrid);
    //}

    public void RemoveSlotAndRefill(IInteractTray tray)
    {
        if (tray == null) return;

        int countNull = 0;

        for(int i = 0; i < trays.Length; i++)
        {
            if (i == tray.GetId())
            {
                trays[i].Reload(null);

                currentTrio[i] = null;

                countNull++;
            }
            else
            {
                if(trays[i].GetCurrentModel() == null)
                {
                    countNull++;
                }
            }
        }

        if(countNull >= trays.Length)
        {
            //// all empty, generate new trio
            //currentTrio = spawner.GenerateThree(new DataGrid(1, 1));
            //for (int i = 0; i < trays.Length; i++)
            //{
            //    trays[i].Reload(currentTrio[i]);
            //}
            //return;
        }
    }

    public BlockModel[] GetCurrentDataTrays()
    {
        return currentTrio;
    } 

    private void OnGridChange(EventChangedGrid eventChangedGrid)
    {
        //Debug.LogError("!111");

        for (int i = 0; i < trays.Length; i++)
        {
            if (trays[i].GetCurrentModel() != null)
            {
                return;
            }
        }

        //Debug.LogError("!222");

        currentTrio = spawner.GenerateThree(eventChangedGrid.dataGrid);

        for (int i = 0; i < trays.Length; i++)
        {
            trays[i].Reload(currentTrio[i], eventChangedGrid.dataGrid);
        }

        //Debug.LogError("!333");
    }

    public BlockModel GetModelAt(int idx)
    {
        if (currentTrio == null || idx < 0 || idx >= currentTrio.Length) return null;
        return currentTrio[idx];
    }
}

public struct EventTrayShapePlaced: IGameEvent
{
    public IInteractTray iInteractTray;

    public EventTrayShapePlaced(IInteractTray tray)
    {
        iInteractTray = tray;
    }
}