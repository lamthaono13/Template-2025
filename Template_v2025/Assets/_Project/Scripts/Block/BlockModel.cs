[System.Serializable]
public class BlockModel
{
    public ShapeData shape;
    public BlockColor color;
    public int id;

    public BlockModel(ShapeData s, BlockColor c, int id = 0)
    {
        shape = s;
        color = c;
        this.id = id;
    }
}
