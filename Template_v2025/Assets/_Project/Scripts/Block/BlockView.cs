using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BlockView : MonoBehaviour
{
    private SpriteRenderer sr;

    [SerializeField] private Sprite[] spritesColors;

    [SerializeField] private SpriteRenderer srShine;
    [SerializeField] private SpriteRenderer srBrick;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        SetAlpha(1f);
        ActiveShine(false);
        ActiveBrick(false);
    }

    public void SetColor(BlockColor color)
    {
        sr.sprite = ColorFor(color);
    }

    public void SetAlpha(float a)
    {
        var c = sr.color;
        c.a = Mathf.Clamp01(a);
        sr.color = c;
    }

    private Sprite ColorFor(BlockColor c)
    {
        return spritesColors[(int)c];
    }

    public void ActiveShine(bool isTrue)
    {
        srShine.gameObject.SetActive(isTrue);
    }

    public void ActiveBrick(bool isTrue)
    {
        srBrick.gameObject.SetActive(isTrue);
    }

    public void SetOrderLayer(int order)
    {
        sr.sortingOrder = order;
        srShine.sortingOrder = order + 1;
        srBrick.sortingOrder = order + 1;
    }
}
