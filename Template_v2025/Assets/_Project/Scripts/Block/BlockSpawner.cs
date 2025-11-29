using System;
using System.Collections.Generic;
using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
    [Header("Assign ShapeData assets (41 ideally)")]
    public ShapeData[] shapePool;

    [Header("Colors available")]
    public BlockColor[] availableColors = new[] { BlockColor.Red, BlockColor.Green, BlockColor.Blue };

    public bool useAdvancedOption = false;

    private System.Random rng = new System.Random();

    public BlockModel[] GenerateThree(GridController grid)
    {
        if (shapePool == null || shapePool.Length == 0)
            throw new Exception("shapePool empty");

        var trio = new BlockModel[3];

        for (int i = 0; i < 3; i++)
        {
            var s = shapePool[rng.Next(shapePool.Length)];
            trio[i] = new BlockModel(s, availableColors[i % availableColors.Length], i);
        }

        // try to ensure individual placeable if advanced
        if (useAdvancedOption)
        {
            for (int attempt = 0; attempt < 200; attempt++)
            {
                for (int i = 0; i < 3; i++)
                    trio[i].shape = shapePool[rng.Next(shapePool.Length)];

                bool ok = true;
                foreach (var b in trio)
                    if (!grid.HasAnyValidPlacement(b.shape)) { ok = false; break; }

                if (ok) break;
            }
        }

        return trio;
    }
}
