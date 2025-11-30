using Sirenix.OdinInspector;
using UnityEngine;

[ExecuteAlways]
public class BoardScaler : MonoBehaviour
{
    public GridRenderer gridRenderer;
    public GameObject blockPrefab;

    [Range(0.4f, 0.95f)]
    public float boardHeightRatio = 0.78f;
    public float horizontalPadding = 0.2f;
    public float verticalOffset = -0.2f;

    [ReadOnly] public float finalScale = 1f;
    [ReadOnly] public float cellSize = 1f;

    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        ApplyScale();
    }

    private void Start()
    {
        ApplyScale();
    }

    [ContextMenu("ApplyScale")]
    public void ApplyScale()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) { Debug.LogError("[BoardScaler] Missing SpriteRenderer or sprite on Board."); return; }
        if (Camera.main == null) { Debug.LogError("[BoardScaler] No main camera found."); return; }

        Vector2 spriteSize = sr.sprite.bounds.size;

        float worldHeight = Camera.main.orthographicSize * 2f;
        float worldWidth = worldHeight * Screen.width / (float)Screen.height;

        float targetHeight = worldHeight * boardHeightRatio;
        float heightScale = targetHeight / spriteSize.y;
            
        float maxAllowedWidth = Mathf.Max(0.1f, worldWidth - horizontalPadding);
        float widthScale = maxAllowedWidth / spriteSize.x;

        finalScale = Mathf.Min(heightScale, widthScale);

        transform.localScale = Vector3.one * finalScale;

        Vector3 pos = transform.position;
        pos.x = 0f;
        pos.y = verticalOffset;
        transform.position = pos;

        //float boardWorldWidth = spriteSize.x * finalScale;
        //cellSize = boardWorldWidth / 8f;

        //if (gridRenderer != null)
        //{
        //    gridRenderer.cellSize = cellSize;
        //}

        //if (blockPrefab != null && gridRenderer != null)
        //{
        //    var br = blockPrefab.GetComponent<SpriteRenderer>();
        //    if (br != null && br.sprite != null)
        //    {
        //        float blockSpriteWidth = br.sprite.bounds.size.x;
        //        float requiredScale = cellSize / blockSpriteWidth;
        //        gridRenderer.blockLocalScale = requiredScale;
        //    }
        //    else
        //    {
        //        Debug.LogWarning("[BoardScaler] blockPrefab missing SpriteRenderer or sprite.");
        //    }
        //}

        Debug.Log($"[BoardScaler] finalScale={finalScale:F3}, cellSize={cellSize:F3}");
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying) ApplyScale();
    }
#endif
}
