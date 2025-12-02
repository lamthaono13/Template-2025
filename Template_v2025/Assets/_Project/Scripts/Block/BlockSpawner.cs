using System;
using System.Collections.Generic;
using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
    [Header("Assign ShapeData assets (41 ideally)")]
    [SerializeField] private ShapeData[] shapePool;

    [Header("Colors available")]
    [SerializeField] private BlockColor[] availableColors;

    public bool useAdvancedOption = false;

    public void Init()
    {

    }

    public BlockModel[] GenerateThree(DataGrid dataGrid)
    {
        if (shapePool == null || shapePool.Length == 0)
            throw new Exception("shapePool empty");

        var trio = new BlockModel[3];

        List<BlockColor> availableColor = new List<BlockColor>(availableColors);

        for (int i = 0; i < 3; i++)
        {
            var s = shapePool[UnityEngine.Random.Range(0, shapePool.Length)];

            int indexColorRandom = UnityEngine.Random.Range(0, availableColor.Count);

            trio[i] = new BlockModel(s, availableColors[indexColorRandom], i);

            availableColor.RemoveAt(indexColorRandom);
        }

        // try to ensure individual placeable if advanced
        if (useAdvancedOption)
        {
            for (int attempt = 0; attempt < 200; attempt++)
            {
                for (int i = 0; i < 3; i++)
                    trio[i].shape = shapePool[UnityEngine.Random.Range(0, shapePool.Length)];

                bool ok = true;
                foreach (var b in trio)
                    if (!GameHelper.HasAnyValidPlacement(b.shape, dataGrid)) { ok = false; break; }

                if (ok) break;
            }
        }

        return trio;
    }
}
