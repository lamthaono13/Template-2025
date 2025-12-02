using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BlockView : MonoBehaviour
{
    private SpriteRenderer sr;

    [SerializeField] private Sprite[] spritesColors;

    [SerializeField] private SpriteRenderer srShine;
    [SerializeField] private SpriteRenderer srBrick;

    [SerializeField] private ParticleSystem particleSystemClear;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        SetAlpha(1f);
        particleSystemClear.gameObject.SetActive(false);
        particleSystemClear.Clear();
        SetAlphaShine(1);
        SetAlphaBrick(1);
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

    public void SetAlphaShine(float a)
    {
        var c = srShine.color;
        c.a = Mathf.Clamp01(a);
        srShine.color = c;
    }

    public void SetAlphaBrick(float a)
    {
        var c = srBrick.color;
        c.a = Mathf.Clamp01(a);
        srBrick.color = c;
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

    public void PlayVfxClear()
    {
        particleSystemClear.gameObject.SetActive(true);

        particleSystemClear.Play();
    }

    public bool CheckIsBrick()
    {
        return srBrick.gameObject.activeSelf;
    }
}
