using System;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour
{
    public const int Width = 8;
    public const int Height = 8;

    public event Action<DataClear> OnCleared;
    public event Action OnGridChanged;

    [SerializeField] private GridRenderer gridRenderer;

    [SerializeField] public PlacementPreviewController placementPreviewController;

    private DataGrid dataGrid;

    public void Init()
    {
        EventBus.AddListener<StartDragEvent>(OnStartDrag);
        EventBus.AddListener<ContinueDragEvent>(OnContinueDrag);
        EventBus.AddListener<EndDragEvent>(OnEndDrag);

        ClearGrid();

        gridRenderer.Init(Width, Height);

        placementPreviewController.Init(Width, Height);

        EventBus.Raise(new EventChangedGrid(dataGrid));
    }

    void OnDestroy()
    {
        EventBus.RemoveListener<StartDragEvent>(OnStartDrag);
        EventBus.RemoveListener<ContinueDragEvent>(OnContinueDrag);
        EventBus.RemoveListener<EndDragEvent>(OnEndDrag);
    }

    private void OnStartDrag(StartDragEvent startDragEvent)
    {
        prevOx = int.MinValue;
        prevOy = int.MinValue;
    }

    int prevOx = int.MinValue;
    int prevOy = int.MinValue;

    private void OnContinueDrag(ContinueDragEvent continueDragEvent)
    {
        int ox, oy;

        Vector3 posContain = continueDragEvent.pos + continueDragEvent.offSet;

        Vector3 posWorldRoot = posContain + new Vector3(continueDragEvent.iInteractTray.GetOffSetTray().x, continueDragEvent.iInteractTray.GetOffSetTray().y, 0);

        bool inside = GameHelper.WorldToGrid(posWorldRoot, placementPreviewController.GetPreviewRoot(), GameHelper.DefaultCellSize, Width, Height, out ox, out oy);



        if (ox == prevOx && oy == prevOy) return;

        prevOx = ox;
        prevOy = oy;

        //Debug.LogError(ox + " " + oy);

        var model = continueDragEvent.iInteractTray.GetCurrentModel();
        if (inside && model != null)
        {
            bool canPlace = GameHelper.CanPlaceShape(model.shape, dataGrid, ox, oy);

            if (canPlace)
            {
                DataClear dataClear = CheckClear(model.shape, model.color, ox, oy);

                placementPreviewController.ShowPreview(model.shape, dataClear, ox, oy, model.color);
            }
            else
            {
                placementPreviewController.ClearPreview();
            }

            //preview.ShowPreview(model.shape, ox, oy, canPlace, model.color);
        }
        else
        {
            placementPreviewController.ClearPreview();
        }
    }

    private void OnEndDrag(EndDragEvent endDragEvent)
    {
        int ox, oy;

        Vector3 posContain = endDragEvent.pos + endDragEvent.offSet;

        Vector3 posWorldRoot = posContain + new Vector3(endDragEvent.iInteractTray.GetOffSetTray().x, endDragEvent.iInteractTray.GetOffSetTray().y, 0);

        bool inside = GameHelper.WorldToGrid(posWorldRoot, placementPreviewController.GetPreviewRoot(), GameHelper.DefaultCellSize, Width, Height, out ox, out oy);

        placementPreviewController.ClearPreview();

        var model = endDragEvent.iInteractTray.GetCurrentModel();

        bool canPlace = GameHelper.CanPlaceShape(model.shape, dataGrid, ox, oy);

        if (inside && model != null && canPlace)
        {
            if (model == null) return;

            if (canPlace)
            {
                EventBus.Raise(new EventTrayShapePlaced(endDragEvent.iInteractTray));

                PlaceShape(model.shape, model.color, ox, oy);

                endDragEvent.iInteractTray.ReturnToTray(false);
            }
            else
            {
                endDragEvent.iInteractTray.ReturnToTray(true);
            }
        }
        else
        {
            endDragEvent.iInteractTray.ReturnToTray(true);
        }
    }

    public void ClearGrid()
    {
        dataGrid = new DataGrid(Width, Height);
        OnGridChanged?.Invoke();
    }

    public void PlaceShape(ShapeData shape, BlockColor color, int ox, int oy)
    {
        foreach (var c in shape.cells)
        {
            int x = ox + c.x;
            int y = oy + c.y;

            if (!GameHelper.IsInside(x, y, dataGrid.Width, dataGrid.Height))
            {
                Debug.LogError($"PlaceShape out of bounds: {x},{y}");
                continue;
            }

            dataGrid.Cells[x, y] = new BlockCell
            {
                color = color
            };
        }

        DataClear cleared = CheckClear();

        if (cleared.Count() > 0)
        {
            ClearFullLines(CheckClear());

            EventBus.Raise(new EventClear(cleared));
        }

        EventBus.Raise(new EventChangedGrid(dataGrid));
    }

    private void ClearFullLines(DataClear dataClear)
    {
        // clear
        foreach (int y in dataClear.ClearRows)
        {
            for (int x = 0; x < Width; x++)
            {
                dataGrid.Cells[x, y] = null;
            }
        }

        foreach (int x in dataClear.ClearCols)
        {
            for (int y = 0; y < Height; y++)
            {
                dataGrid.Cells[x, y] = null;
            }
        }

    }

    public DataClear CheckClear()
    {
        List<int> rowsToClear = new List<int>();
        List<int> colsToClear = new List<int>();

        for (int y = 0; y < Height; y++)
        {
            bool full = true;
            for (int x = 0; x < Width; x++)
            {
                if (dataGrid.Cells[x, y] == null) { full = false; break; }
            }
            if (full) rowsToClear.Add(y);
        }

        for (int x = 0; x < Width; x++)
        {
            bool full = true;
            for (int y = 0; y < Height; y++)
            {
                if (dataGrid.Cells[x, y] == null) { full = false; break; }
            }
            if (full) colsToClear.Add(x);
        }

        DataClear dataClear = new DataClear
        (
            rowsToClear.ToArray(),
            colsToClear.ToArray()
        );

        return dataClear;
    }

    public DataClear CheckClear(ShapeData shape, BlockColor color, int ox, int oy)
    {
        BlockCell[,] tempCell = GetCellsSnapshot();

        foreach (var c in shape.cells)
        {
            int x = ox + c.x;
            int y = oy + c.y;

            if (!GameHelper.IsInside(x, y, dataGrid.Width, dataGrid.Height))
            {
                //Debug.LogError($"PlaceShape out of bounds: {x},{y}");
                continue;
            }

            tempCell[x, y] = new BlockCell
            {
                color = color
            };
        }

        List<int> rowsToClear = new List<int>();
        List<int> colsToClear = new List<int>();

        for (int y = 0; y < Height; y++)
        {
            bool full = true;
            for (int x = 0; x < Width; x++)
            {
                if (tempCell[x, y] == null) { full = false; break; }
            }
            if (full) rowsToClear.Add(y);
        }

        for (int x = 0; x < Width; x++)
        {
            bool full = true;
            for (int y = 0; y < Height; y++)
            {
                if (tempCell[x, y] == null) { full = false; break; }
            }
            if (full) colsToClear.Add(x);
        }

        DataClear dataClear = new DataClear
        (
            rowsToClear.ToArray(),
            colsToClear.ToArray()
        );

        rowsToClear.Clear();
        colsToClear.Clear();

        return dataClear;
    }

    //public bool AnyOfShapesPlaceable(IEnumerable<ShapeData> shapes)
    //{
    //    foreach (var s in shapes) if (HasAnyValidPlacement(s)) return true;
    //    return false;
    //}

    public BlockCell[,] GetCellsSnapshot()
    {
        var snap = new BlockCell[Width, Height];
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                snap[x, y] = dataGrid.Cells[x, y] == null ? null : new BlockCell { color = dataGrid.Cells[x, y].color };
            }
        }

        return snap;
    }
}

[System.Serializable]
public class BlockCell
{
    public BlockColor color;
}

[System.Serializable]
public struct EventClear: IGameEvent
{
    public DataClear dataClear;

    public EventClear(DataClear dataClear)
    {
        this.dataClear = dataClear;
    }
}

public class DataClear
{
    public int[] ClearRows;
    public int[] ClearCols;

    public DataClear(int[] clearRows, int[] clearCols)
    {
        ClearRows = clearRows;
        ClearCols = clearCols;
    }

    public int Count()
    {
        return ClearRows.Length + ClearCols.Length;
    }
}

public struct EventChangedGrid : IGameEvent
{
    public DataGrid dataGrid;
    public EventChangedGrid(DataGrid grid)
    {
        dataGrid = grid;
    }
}

public class DataGrid
{
    public BlockCell[,] Cells;

    public int Width;

    public int Height;

    public DataGrid(int width, int height)
    {
        Width = width;

        Height = height;

        Cells = new BlockCell[width, height];
    }
}