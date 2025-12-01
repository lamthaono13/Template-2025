using System.Collections.Generic;
using UnityEngine;


public class TrayManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject blockPrefab;                 // piece prefab (1 tile)
    [SerializeField] private Tray[] trays;
    [SerializeField] private GameManager gameManager;
    [Tooltip("Scale of tray shapes relative to board cell size")]
    public float visualScale = 0.6f;

    [HideInInspector]
    public BlockModel[] currentTrio = new BlockModel[0];



    private BlockSpawner spawner;

    private void Awake()
    {
        // allocate arrays
        int slots = Mathf.Max(3, trays.Length);
    }

    private void Start()
    {

    }

    public void Init()
    {
        if (spawner == null)
        {
            spawner = gameManager.Spawner;
        }

        GenerateInitial();

        for (int i = 0; i < trays.Length; i++)
        {
            trays[i].Init(i, currentTrio[i], this, gameManager);
        }


    }

    public void GenerateInitial()
    {
        if (spawner == null)
        {
            Debug.LogWarning("[TrayManager] spawner not assigned");
            return;
        }

        // generate trio using spawner
        currentTrio = spawner.GenerateThree(gameManager.Grid);
    }

    public void RemoveSlotAndRefill(Tray tray, bool isPlace)
    {
        if (!isPlace) return;

        if (tray == null) return;

        int countNull = 0;

        for(int i = 0; i < trays.Length; i++)
        {
            if (trays[i] == tray)
            {
                trays[i].Reload(null);

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
            // all empty, generate new trio
            currentTrio = spawner.GenerateThree(gameManager.Grid);
            for (int i = 0; i < trays.Length; i++)
            {
                trays[i].Reload(currentTrio[i]);
            }
            return;
        }
    }

    public BlockModel GetModelAt(int idx)
    {
        if (currentTrio == null || idx < 0 || idx >= currentTrio.Length) return null;
        return currentTrio[idx];
    }
}