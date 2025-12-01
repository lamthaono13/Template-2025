using DG.Tweening;
using UnityEngine;

public class WorldDragController : MonoBehaviour
{
    [SerializeField] private Camera cam;
    private TrayManager trayManager;
    private PlacementPreviewController preview;
    [SerializeField] private GameManager gameManager;
    private GridRenderer gridRenderer;

    private bool isDragging = false;
    private GameObject draggingVisual = null;
    private Vector3 dragOffset = Vector3.zero;
    [SerializeField]private Vector3 dragOffsetDefault = Vector3.zero;
    public int dragSortingOrder = 1000;

    private Tray currTray;

    private bool canDrag = false;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    public void Init()
    {
        canDrag = true;

        trayManager = gameManager.TrayManager;
        preview = gameManager.Preview;
        gridRenderer = gameManager.Grid.GridRenderer;
    }

    private void Update()
    {
        if (!canDrag)
        {
            return;
        }

        if (Input.touchCount > 0)
        {
            var t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began) TryBeginDrag(t.position);
            else if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary) ContinueDrag(t.position);
            else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) EndDrag(t.position);
        }
        else
        {
            if (Input.GetMouseButtonDown(0)) TryBeginDrag(Input.mousePosition);
            else if (Input.GetMouseButton(0)) ContinueDrag(Input.mousePosition);
            else if (Input.GetMouseButtonUp(0)) EndDrag(Input.mousePosition);
        }
    }

    private void TryBeginDrag(Vector3 screenPos)
    {
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Mathf.Abs(cam.transform.position.z)));
        Vector2 p = new Vector2(world.x, world.y);
        RaycastHit2D hit = Physics2D.Raycast(p, Vector2.zero, 0f);
        if (hit.collider == null) return;

        if(!hit.collider.CompareTag("Tray"))
        {
            return;
        }

        currTray = hit.collider.GetComponent<Tray>();

        if (!currTray.CanGet())
        {
            return;
        }

        BeginDragForSlot(world);
    }


    private void BeginDragForSlot(Vector3 worldPos)
    {
        if (isDragging) return;

        var model = currTray.GetCurrentModel();
        if (model == null) return;

        var container = currTray.GetContainer();
        if (container == null)
        {
            return;
        }

        isDragging = true;


        // clone the whole container
        draggingVisual = container;


        dragOffset = draggingVisual.transform.position - worldPos;

        Vector3 initialPos = new Vector3(worldPos.x + dragOffset.x + dragOffsetDefault.x, worldPos.y + dragOffset.y + dragOffsetDefault.y, draggingVisual.transform.position.z);

        container.transform.DOMove(initialPos, 0.2f).SetEase(DG.Tweening.Ease.Linear);

        //ContinueDragImmediate(worldPos);
    }

    private void ContinueDrag(Vector3 screenPos)
    {
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Mathf.Abs(cam.transform.position.z)));
        ContinueDragImmediate(world);
    }

    private int prevOx = -1;
    private int prevOy = -1;

    private void ContinueDragImmediate(Vector3 world)
    {
        if (!isDragging || draggingVisual == null) return;

        draggingVisual.transform.position = new Vector3(world.x + dragOffset.x + dragOffsetDefault.x, world.y + dragOffset.y + dragOffsetDefault.y, draggingVisual.transform.position.z);

        int ox, oy;
        bool inside = gameManager.Grid.GridRenderer.WorldToGrid(draggingVisual.transform.position, out ox, out oy);

        if(ox == prevOx && oy == prevOy) return;

        prevOx = ox;
        prevOy = oy;

        var model = currTray.GetCurrentModel();
        if (inside && model != null)
        {
            bool canPlace = gameManager.Grid.CanPlaceShape(model.shape, ox, oy);

            if (canPlace)
            {
                preview.ShowPreview(model.shape, ox, oy, canPlace, model.color);
            }
            else
            {
                preview.ClearPreview();
            }

            //preview.ShowPreview(model.shape, ox, oy, canPlace, model.color);
        }
        else
        {
            preview.ClearPreview();
        }
    }

    private void EndDrag(Vector3 screenPos)
    {
        if (!isDragging) return;

        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Mathf.Abs(cam.transform.position.z)));
        Vector3 checkPos = draggingVisual != null ? draggingVisual.transform.position : world;

        int ox, oy;
        bool inside = gridRenderer.WorldToGrid(checkPos, out ox, out oy);

        preview.ClearPreview();

        var model = currTray.GetCurrentModel();

        if (inside && model != null && gameManager.Grid.CanPlaceShape(model.shape, ox, oy))
        {
            //gameManager.TryPlaceFromUI(model, ox, oy);


            if (model == null) return;

            if (gameManager.Grid.CanPlaceShape(model.shape, ox, oy))
            {
                gameManager.Grid.PlaceShape(model.shape, model.color, ox, oy);

                currTray.ReturnToTray(false);

                trayManager.RemoveSlotAndRefill(currTray, true);
            }
            else
            {
                currTray.ReturnToTray(true);

                trayManager.RemoveSlotAndRefill(currTray, false);
            }
        }
        else
        {
            currTray.ReturnToTray(true);
        }

        draggingVisual = null;
        isDragging = false;
        currTray = null;
    }
}
