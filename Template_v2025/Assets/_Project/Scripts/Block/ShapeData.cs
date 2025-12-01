using UnityEngine;

[CreateAssetMenu(menuName = "BlockSmash/ShapeData")]
public class ShapeData : ScriptableObject
{
    public BlockShape BlockShape;

    public string shapeName;

    public Vector2Int[] cells
    {
        get
        {
            var cellList = new System.Collections.Generic.List<Vector2Int>();
            for (int x = 0; x < BlockShape.width; x++)
            {
                for (int y = 0; y < BlockShape.height; y++)
                {
                    if (BlockShape.IsPartOfShape(new Vector2Int(x, y)))
                    {
                        cellList.Add(new Vector2Int(x, y));
                    }
                }
            }
            return cellList.ToArray();
        }
    }
}
