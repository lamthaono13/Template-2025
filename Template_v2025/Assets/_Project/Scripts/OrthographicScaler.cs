using UnityEngine;
using System.Collections.Generic;


public enum Anchor
{
    TopLeft,
    TopCenter,
    TopRight,
    MiddleLeft,
    Center,
    MiddleRight,
    BottomLeft,
    BottomCenter,
    BottomRight
}


[ExecuteAlways]
[DisallowMultipleComponent]
public class OrthographicScaler : MonoBehaviour
{
    [Header("Camera & Reference")]
    public Camera targetCamera; // if null, Camera.main is used
    public Vector2 referenceResolution = new Vector2(1920, 1080);


    [Header("Scale Options")]
    public bool scaleByWidth = true;
    public bool scaleByHeight = false;
    public bool keepAspectWhenBoth = true; // when both true, uses min scale


    [Header("Anchor & Position")]
    public Anchor anchor = Anchor.Center;
    public Vector2 pixelOffset = Vector2.zero; // offset in pixels relative to anchor


    [Header("Runtime")]
    public bool updateEveryFrame = false; // for editor convenience set true


    Vector3 _originalLocalScale;
    bool _initialized = false;


    void OnEnable()
    {
        Initialize();
        Apply();
    }


    void OnValidate()
    {
        Initialize();
        Apply();
    }


    void Start()
    {
        Initialize();
        Apply();
    }


    void Update()
    {
        if (updateEveryFrame)
            Apply();
    }


    void Initialize()
    {
        if (_initialized) return;
        _originalLocalScale = Vector3.one;
        if (targetCamera == null) targetCamera = Camera.main;
        _initialized = true;
    }


    public void Apply()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera == null) return;


        // compute scale factors
        float wScale = (float)Screen.width / Mathf.Max(1f, referenceResolution.x);
        float hScale = (float)Screen.height / Mathf.Max(1f, referenceResolution.y);


        float chosenScale = 1f;
        if (scaleByWidth && scaleByHeight)
        {
            chosenScale = keepAspectWhenBoth ? Mathf.Min(wScale, hScale) : Mathf.Max(wScale, hScale);
        }
        else if (scaleByWidth) chosenScale = wScale;
        else if (scaleByHeight) chosenScale = hScale;


        transform.localScale = _originalLocalScale * chosenScale;


        // compute anchor viewport coords
        Vector2 vp = AnchorToViewport(anchor);


        // viewport to world (use object's z for depth)
        float z = transform.position.z;
        // For orthographic camera, the world Z doesn't affect X/Y from ViewportToWorldPoint in the same way - we use camera's plane
        Vector3 anchorWorld = targetCamera.ViewportToWorldPoint(new Vector3(vp.x, vp.y, Mathf.Abs(targetCamera.transform.position.z - z)));


        // convert pixel offset -> world units. For orthographic camera, 1 world unit = camera.orthographicSize*2 in Y equals Screen.height pixels
        float pixelsPerUnit = (targetCamera.orthographicSize * 2f) / Screen.height; // world units per pixel
        Vector3 worldOffset = new Vector3(pixelOffset.x * pixelsPerUnit, pixelOffset.y * pixelsPerUnit, 0f);


        Vector3 targetPos = new Vector3(anchorWorld.x + worldOffset.x, anchorWorld.y + worldOffset.y, z);
        transform.position = targetPos;
    }


    static Vector2 AnchorToViewport(Anchor a)
    {
        switch (a)
        {
            case Anchor.TopLeft: return new Vector2(0f, 1f);
            case Anchor.TopCenter: return new Vector2(0.5f, 1f);
            case Anchor.TopRight: return new Vector2(1f, 1f);


            case Anchor.MiddleLeft: return new Vector2(0f, 0.5f);
            case Anchor.Center: return new Vector2(0.5f, 0.5f);
            case Anchor.MiddleRight: return new Vector2(1f, 0.5f);


            case Anchor.BottomLeft: return new Vector2(0f, 0f);
            case Anchor.BottomCenter: return new Vector2(0.5f, 0f);
            case Anchor.BottomRight: return new Vector2(1f, 0f);
        }
        return Vector2.zero;
    }
}