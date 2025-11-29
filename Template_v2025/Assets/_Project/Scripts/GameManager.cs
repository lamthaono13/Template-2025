using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public GridController grid;
    public BlockSpawner spawner;
    public TrayManager trayManager;
    public PlacementPreviewController preview;
    public GridRenderer gridRenderer;

    private void Start()
    {
        grid.OnGridChanged += OnGridChanged;
        grid.OnRowsCleared += OnRowsCleared;

        if (trayManager != null) trayManager.spawner = spawner;

        if (trayManager != null && (trayManager.currentTrio == null || trayManager.currentTrio.Length == 0))
            trayManager.GenerateInitial();
    }

    private void OnDestroy()
    {
        grid.OnGridChanged -= OnGridChanged;
        grid.OnRowsCleared -= OnRowsCleared;
    }

    private void OnGridChanged()
    {
        var snap = grid.GetCellsSnapshot();
        if (gridRenderer != null) gridRenderer.ApplySnapshot(snap);
    }

    private void OnRowsCleared(int count)
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
