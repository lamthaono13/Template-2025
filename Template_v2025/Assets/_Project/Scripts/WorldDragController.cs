using DG.Tweening;
using UnityEngine;

public class WorldDragController : MonoBehaviour
{
    [SerializeField] private Camera cam;
    //private TrayManager trayManager;
    //private PlacementPreviewController preview;
    //[SerializeField] private GameManager gameManager;
    //private GridRenderer gridRenderer;

    private bool isDragging = false;

    private Vector3 dragOffset = Vector3.zero;
    [SerializeField]private Vector3 dragOffsetDefault = Vector3.zero;

    private IInteractTray currTray;

    private bool canDrag = false;

    private void Start()
    {
        EventBus.AddListener<EventEndGame>(OnEndGame);
    }

    private void OnDestroy()
    {
        EventBus.RemoveListener<EventEndGame>(OnEndGame);
    }

    private void OnEndGame(EventEndGame eventEndGame)
    {
        canDrag = false;
    }

    public void Init()
    {
        canDrag = true;

        if (cam == null) cam = Camera.main;
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

        currTray = hit.collider.GetComponent<IInteractTray>();

        if (!currTray.CanGetTray())
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


        dragOffset = container.transform.position - worldPos;

        EventBus.Raise(new StartDragEvent(worldPos, dragOffset + dragOffsetDefault, currTray));

        ContinueDragImmediate(worldPos);
    }

    private void ContinueDrag(Vector3 screenPos)
    {
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Mathf.Abs(cam.transform.position.z)));
        ContinueDragImmediate(world);
    }

    private void ContinueDragImmediate(Vector3 world)
    {
        if (!isDragging || currTray == null) return;

        EventBus.Raise(new ContinueDragEvent(world, dragOffset + dragOffsetDefault, currTray));
    }

    private void EndDrag(Vector3 screenPos)
    {
        if (!isDragging) return;

        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Mathf.Abs(cam.transform.position.z)));

        EventBus.Raise(new EndDragEvent(world, dragOffset + dragOffsetDefault, currTray));

        isDragging = false;
        currTray = null;
    }
}


public struct StartDragEvent : IGameEvent
{
    public Vector2 pos;

    public Vector2 offSet;

    public IInteractTray iInteractTray;

    public StartDragEvent(Vector2 pos, Vector2 offSet, IInteractTray iInteractTray)
    {
        this.pos = pos;
        this.offSet = offSet;
        this.iInteractTray = iInteractTray;
    }
}

public struct ContinueDragEvent : IGameEvent
{
    public Vector2 pos;

    public Vector2 offSet;

    public IInteractTray iInteractTray;

    public ContinueDragEvent(Vector2 pos, Vector2 offSet, IInteractTray iInteractTray)
    {
        this.pos = pos;
        this.offSet = offSet;
        this.iInteractTray = iInteractTray;
    }
}

public struct EndDragEvent : IGameEvent
{
    public Vector2 pos;

    public Vector2 offSet;

    public IInteractTray iInteractTray;

    public EndDragEvent(Vector2 pos, Vector2 offSet, IInteractTray iInteractTray)
    {
        this.pos = pos;
        this.offSet = offSet;
        this.iInteractTray = iInteractTray;
    }
}