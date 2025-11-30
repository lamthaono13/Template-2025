using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BlockView : MonoBehaviour
{
    private SpriteRenderer sr;

    [SerializeField] private Sprite[] spritesColors;

    [SerializeField] private GameObject objShine;
    [SerializeField] private GameObject objBrick;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
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
        objShine.gameObject.SetActive(isTrue);
    }

    public void ActiveBrick(bool isTrue)
    {
        objBrick.gameObject.SetActive(isTrue);
    }
}
