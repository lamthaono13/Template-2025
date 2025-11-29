using UnityEngine;

public class WorldDragController : MonoBehaviour
{
    public Camera cam;
    public TrayManager trayManager;
    public PlacementPreviewController preview;
    public GameManager gameManager;
    public GridRenderer gridRenderer;

    private bool isDragging = false;
    private GameObject draggingVisual = null;
    private int draggingSlotIndex = -1;
    private Vector3 dragOffset = Vector3.zero;
    public int dragSortingOrder = 1000;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    private void Update()
    {
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

        GameObject hitGo = hit.collider.gameObject;
        int idx = FindSlotIndexByInstance(hitGo);
        if (idx == -1) return;

        BeginDragForSlot(idx, hitGo, world);
    }

    private int FindSlotIndexByInstance(GameObject instance)
    {
        // look for parent container named TrayShape_i
        Transform t = instance.transform;
        while (t != null)
        {
            if (t.name.StartsWith("TrayShape_"))
            {
                string s = t.name.Replace("TrayShape_", "");
                if (int.TryParse(s, out int parsed)) return parsed;
            }
            t = t.parent;
        }

        // fallback position match
        if (trayManager != null)
        {
            for (int i = 0; i < trayManager.slotTransforms.Length; i++)
            {
                if (trayManager.slotTransforms[i] == null) continue;
                if (Vector3.Distance(instance.transform.position, trayManager.slotTransforms[i].position) < 0.05f) return i;
            }
        }

        return -1;
    }


    private void BeginDragForSlot(int slotIndex, GameObject slotInstance, Vector3 worldPos)
    {
        if (isDragging) return;

        var model = trayManager.GetModelAt(slotIndex);
        if (model == null) return;

        var container = trayManager.slotContainers[slotIndex];
        if (container == null)
        {
            Debug.LogWarning("[WorldDragController] slot container missing for index " + slotIndex);
            return;
        }

        isDragging = true;
        draggingSlotIndex = slotIndex;

        // clone the whole container
        draggingVisual = Instantiate(container, container.transform.position, container.transform.rotation);
        draggingVisual.name = "DraggingVisual";
        draggingVisual.transform.SetParent(null, true);

        // disable colliders on clone and raise sorting order
        foreach (var c in draggingVisual.GetComponentsInChildren<Collider2D>()) c.enabled = false;
        foreach (var sr in draggingVisual.GetComponentsInChildren<SpriteRenderer>()) sr.sortingOrder = dragSortingOrder;

        // hide original container
        container.SetActive(false);

        dragOffset = draggingVisual.transform.position - worldPos;

        ContinueDragImmediate(worldPos);
    }

    private void ContinueDrag(Vector3 screenPos)
    {
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Mathf.Abs(cam.transform.position.z)));
        ContinueDragImmediate(world);
    }

    private void ContinueDragImmediate(Vector3 world)
    {
        if (!isDragging || draggingVisual == null) return;

        draggingVisual.transform.position = new Vector3(world.x + dragOffset.x, world.y + dragOffset.y, draggingVisual.transform.position.z);

        int ox, oy;
        bool inside = gridRenderer.WorldToGrid(draggingVisual.transform.position, out ox, out oy);

        var model = trayManager.GetModelAt(draggingSlotIndex);
        if (inside && model != null)
        {
            bool canPlace = gameManager.grid.CanPlaceShape(model.shape, ox, oy);

            if (canPlace)
            {
                preview.ShowPreview(model.shape, ox, oy, canPlace, model.color);
            }
            else
            {
                preview.ClearPreview();
            }

            preview.ShowPreview(model.shape, ox, oy, canPlace, model.color);
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

        var model = trayManager.GetModelAt(draggingSlotIndex);

        if (inside && model != null && gameManager.grid.CanPlaceShape(model.shape, ox, oy))
        {
            gameManager.TryPlaceFromUI(draggingSlotIndex, ox, oy);
            trayManager.RemoveSlotAndRefill(draggingSlotIndex);
        }
        else
        {
            trayManager.RefreshVisuals();
        }

        if (draggingVisual != null) Destroy(draggingVisual);
        isDragging = false;
        draggingSlotIndex = -1;
    }
}
