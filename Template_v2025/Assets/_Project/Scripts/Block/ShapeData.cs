using UnityEngine;

[CreateAssetMenu(menuName = "BlockSmash/ShapeData")]
public class ShapeData : ScriptableObject
{
    public Vector2Int[] cells;
    public string shapeName;
}
