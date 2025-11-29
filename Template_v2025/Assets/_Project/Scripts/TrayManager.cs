using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TrayManager (optimized):
/// - Pre-creates slot containers (slotContainers)
/// - Each slot has an internal pool of piece GameObjects to reuse when shapes change
/// - RefreshVisuals reuses pooled pieces, only instantiates when pool empty
/// - slotContainers are persistent GameObjects (no create/destroy except piece growth)
/// </summary>
public class TrayManager : MonoBehaviour
{
    [Header("References")]
    public GameObject blockPrefab;                 // piece prefab (1 tile)
    public Transform[] slotTransforms = new Transform[3]; // world positions for slots

    [Tooltip("Scale of tray shapes relative to board cell size")]
    public float visualScale = 0.6f;

    [HideInInspector]
    public BlockModel[] currentTrio = new BlockModel[0];

    // persistent containers (one per slot)
    [HideInInspector]
    public GameObject[] slotContainers;

    // per-slot pools & active lists
    private Stack<GameObject>[] piecePools;
    private List<GameObject>[] activePieces;

    public BlockSpawner spawner;
    public GameManager gameManager;

    private void Awake()
    {
        // allocate arrays
        int slots = Mathf.Max(3, slotTransforms.Length);
        slotContainers = new GameObject[slots];
        piecePools = new Stack<GameObject>[slots];
        activePieces = new List<GameObject>[slots];

        for (int i = 0; i < slots; i++)
        {
            piecePools[i] = new Stack<GameObject>();
            activePieces[i] = new List<GameObject>();
        }
    }

    private void Start()
    {
        if (spawner == null) Debug.LogWarning("[TrayManager] spawner missing");
        InitializeSlotContainers();
        GenerateInitial();
    }

    private void InitializeSlotContainers()
    {
        for (int i = 0; i < slotContainers.Length; i++)
        {
            // create container gameobject as child of TrayManager
            var go = new GameObject($"TrayShape_{i}");
            go.transform.SetParent(this.transform, false);

            // position at desired slot transform if provided
            if (i < slotTransforms.Length && slotTransforms[i] != null)
                go.transform.position = slotTransforms[i].position;
            else
                go.transform.localPosition = new Vector3(i * 1.0f, 0f, 0f); // fallback

            slotContainers[i] = go;
            go.SetActive(false); // initially hidden until populated
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
        currentTrio = spawner.GenerateThree(gameManager.grid);
        RefreshVisuals();
    }

    /// <summary>
    /// Reuse existing containers and piece pools to display shapes.
    /// Does not destroy containers.
    /// </summary>
    public void RefreshVisuals()
    {
        // hide all containers first
        for (int i = 0; i < slotContainers.Length; i++)
        {
            var c = slotContainers[i];
            if (c != null) c.SetActive(false);

            // recycle active pieces into pool
            foreach (var p in activePieces[i])
            {
                if (p != null)
                {
                    p.SetActive(false);
                    piecePools[i].Push(p);
                }
            }
            activePieces[i].Clear();
        }

        // populate visible slots based on currentTrio
        int count = currentTrio != null ? currentTrio.Length : 0;
        for (int i = 0; i < count && i < slotContainers.Length; i++)
        {
            var model = currentTrio[i];
            var container = slotContainers[i];
            if (container == null) continue;

            container.SetActive(true);
            // ensure container at slotTransform position (in case layout changed)
            if (i < slotTransforms.Length && slotTransforms[i] != null)
                container.transform.position = slotTransforms[i].position;

            // compute shape bounds to center it
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            foreach (var cCoord in model.shape.cells)
            {
                if (cCoord.x < minX) minX = cCoord.x;
                if (cCoord.y < minY) minY = cCoord.y;
                if (cCoord.x > maxX) maxX = cCoord.x;
                if (cCoord.y > maxY) maxY = cCoord.y;
            }
            int shapeW = maxX - minX + 1;
            int shapeH = maxY - minY + 1;

            // base cell size from gridRenderer if possible
            float baseCellSize = 1f;
            if (gameManager != null && gameManager.gridRenderer != null)
                baseCellSize = gameManager.gridRenderer.cellSize;

            float visualCellSize = baseCellSize * visualScale;

            Vector2 centerOffset = new Vector2((shapeW - 1) * 0.5f, (shapeH - 1) * 0.5f);

            // place or reuse pieces for this slot
            foreach (var cCoord in model.shape.cells)
            {
                GameObject piece = null;
                if (piecePools[i].Count > 0)
                {
                    piece = piecePools[i].Pop();
                    piece.SetActive(true);
                    piece.transform.SetParent(container.transform, false);
                }
                else
                {
                    // instantiate new piece under container
                    piece = Instantiate(blockPrefab, container.transform);
                    // ensure there's a collider for selection; collider size will be sprite bounds
                    if (piece.GetComponent<Collider2D>() == null)
                    {
                        var col = piece.AddComponent<BoxCollider2D>();
                        var sr = piece.GetComponent<SpriteRenderer>();
                        if (sr != null && sr.sprite != null) col.size = sr.sprite.bounds.size;
                    }
                }

                // set local position to center the shape at container origin
                Vector3 localPos = new Vector3((cCoord.x - minX - centerOffset.x) * visualCellSize,
                                               (cCoord.y - minY - centerOffset.y) * visualCellSize,
                                               0f);

                piece.transform.localPosition = localPos;

                // scale piece so it visually matches grid scale (use gridRenderer.blockLocalScale if available)
                float finalPieceScale = visualScale;
                if (gameManager != null && gameManager.gridRenderer != null)
                {
                    // gridRenderer.blockLocalScale is local scale for root==GridRoot; here we want same visual (but container is in world space)
                    finalPieceScale = visualScale * gameManager.gridRenderer.blockLocalScale;
                }
                piece.transform.localScale = Vector3.one * finalPieceScale;

                // set color/alpha
                var view = piece.GetComponent<BlockView>();
                if (view != null)
                {
                    view.SetColor(model.color);
                    view.SetAlpha(1f);
                }

                // keep in active list for later recycle
                activePieces[i].Add(piece);
            }
        }
    }

    /// <summary>
    /// Called after a successful placement: remove used index and refill if necessary.
    /// </summary>
    public void RemoveSlotAndRefill(int index)
    {
        if (index < 0 || index >= currentTrio.Length) return;

        var list = new System.Collections.Generic.List<BlockModel>(currentTrio);
        list.RemoveAt(index);
        currentTrio = list.ToArray();

        RefreshVisuals();

        if (currentTrio.Length == 0)
        {
            currentTrio = spawner.GenerateThree(gameManager.grid);
            RefreshVisuals();
        }
    }

    public BlockModel GetModelAt(int idx)
    {
        if (currentTrio == null || idx < 0 || idx >= currentTrio.Length) return null;
        return currentTrio[idx];
    }
}