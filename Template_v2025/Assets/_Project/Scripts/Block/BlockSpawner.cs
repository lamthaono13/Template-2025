using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
    [Header("Assign ShapeData assets (41 ideally)")]
    [SerializeField] private ShapeData[] shapePool;

    [Header("Colors available")]
    [SerializeField] private BlockColor[] availableColors;

    private bool useAdvancedOption = false;

    public void Init()
    {

    }

    public BlockModel[] GenerateThree(DataGrid dataGrid)
    {
        if (shapePool == null || shapePool.Length == 0)
            throw new Exception("shapePool empty");

        //var trio = new BlockModel[3];

        //List<BlockColor> availableColor = new List<BlockColor>(availableColors);

        //for (int i = 0; i < 3; i++)
        //{
        //    var s = shapePool[UnityEngine.Random.Range(0, shapePool.Length)];

        //    int indexColorRandom = UnityEngine.Random.Range(0, availableColor.Count);

        //    trio[i] = new BlockModel(s, availableColors[indexColorRandom], i);

        //    availableColor.RemoveAt(indexColorRandom);
        //}

        //// try to ensure individual placeable if advanced
        ///

        bool useAdvancedOption = ShouldUseAdvanced(dataGrid);

        if (useAdvancedOption)
        {
            return GenerateThreeAdvanced(dataGrid);
        }
        else
        {
            return GenerateThreeRandom(dataGrid);
        }
    }

    public BlockModel[] GenerateThreeRandom(DataGrid dataGrid)
    {
        var result = new List<BlockModel>(3);

        // shape placeable
        var placeable = new List<ShapeData>();
        foreach (var s in shapePool)
            if (GameHelper.HasAnyValidPlacement(s, dataGrid))
                placeable.Add(s);

        Shuffle(placeable);
        var usedShapes = new HashSet<ShapeData>();

        // chọn shapes khác nhau trước (logic shape giống cũ)
        foreach (var s in placeable)
        {
            if (usedShapes.Contains(s)) continue;
            usedShapes.Add(s);
            result.Add(new BlockModel(s, BlockColor.Red, 0)); // màu tạm đặt ở đây, sẽ gán lại ngay sau
            if (result.Count == 3) break;
        }

        // nếu thiếu thì lấy shape bất kỳ nhưng không trùng
        if (result.Count < 3)
        {
            var all = new List<ShapeData>(shapePool);
            Shuffle(all);
            foreach (var s in all)
            {
                if (usedShapes.Contains(s)) continue;
                usedShapes.Add(s);
                result.Add(new BlockModel(s, BlockColor.Red, 0)); // màu tạm
                if (result.Count == 3) break;
            }
        }

        // GÁN MÀU: đảm bảo 3 màu khác nhau (nếu possible)
        var distinctColors = GetDistinctColors(result.Count);
        for (int i = 0; i < result.Count; i++)
        {
            // nếu số màu < result.Count, assign vòng lặp (ít nhất dùng tất cả màu)
            result[i].color = distinctColors[i % distinctColors.Count];
        }

        return result.ToArray();
    }

    public BlockModel[] GenerateThreeAdvanced(DataGrid dataGrid)
    {
        // build all valid placements
        var placements = new List<(ShapeData shape, Vector2Int pos, List<Vector2Int> abs)>();

        foreach (var shape in shapePool)
        {
            int count = 0;
            for (int x = 0; x < dataGrid.Width; x++)
            {
                for (int y = 0; y < dataGrid.Height; y++)
                {
                    if (GameHelper.CanPlaceShape(shape, dataGrid, x, y))
                    {
                        placements.Add((shape, new Vector2Int(x, y), GetAbs(shape, x, y)));
                        count++;
                        if (count >= 300) break;
                    }
                }
            }
        }

        if (placements.Count < 3)
            return GenerateThreeRandom(dataGrid);

        // base occupancy
        bool[,] baseOcc = new bool[dataGrid.Width, dataGrid.Height];
        for (int x = 0; x < dataGrid.Width; x++)
            for (int y = 0; y < dataGrid.Height; y++)
                baseOcc[x, y] = dataGrid.Cells[x, y] != null;

        int tries = 4000;
        int n = placements.Count;

        for (int t = 0; t < tries; t++)
        {
            var a = placements[UnityEngine.Random.Range(0, n)];
            var b = placements[UnityEngine.Random.Range(0, n)];
            var c = placements[UnityEngine.Random.Range(0, n)];

            if (!Disjoint(a.abs, b.abs)) continue;
            if (!Disjoint(a.abs, c.abs)) continue;
            if (!Disjoint(b.abs, c.abs)) continue;

            bool[,] sim = (bool[,])baseOcc.Clone();
            Mark(sim, a.abs);
            Mark(sim, b.abs);
            Mark(sim, c.abs);

            if (Clearable(sim, dataGrid.Width, dataGrid.Height))
            {
                // TẠO 3 BlockModel với màu tạm
                var blocks = new BlockModel[]
                {
                    new BlockModel(a.shape, BlockColor.Red, 0),
                    new BlockModel(b.shape, BlockColor.Red, 0),
                    new BlockModel(c.shape, BlockColor.Red, 0)
                };

                // Gán 3 màu khác nhau (nếu có thể)
                var distinctColors = GetDistinctColors(3);
                for (int i = 0; i < blocks.Length; i++)
                    blocks[i].color = distinctColors[i % distinctColors.Count];

                return blocks;
            }
        }

        return GenerateThreeRandom(dataGrid);
    }

    private List<Vector2Int> GetAbs(ShapeData s, int ox, int oy)
    {
        var list = new List<Vector2Int>();
        foreach (var c in s.cells)
            list.Add(new Vector2Int(ox + c.x, oy + c.y));
        return list;
    }

    private bool Disjoint(List<Vector2Int> a, List<Vector2Int> b)
    {
        var set = new HashSet<Vector2Int>(a);
        foreach (var v in b) if (set.Contains(v)) return false;
        return true;
    }

    private void Mark(bool[,] sim, List<Vector2Int> abs)
    {
        foreach (var v in abs)
            sim[v.x, v.y] = true;
    }

    private bool Clearable(bool[,] sim, int w, int h)
    {
        // row
        for (int y = 0; y < h; y++)
        {
            bool full = true;
            for (int x = 0; x < w; x++)
                if (!sim[x, y]) { full = false; break; }
            if (full) return true;
        }

        // col
        for (int x = 0; x < w; x++)
        {
            bool full = true;
            for (int y = 0; y < h; y++)
                if (!sim[x, y]) { full = false; break; }
            if (full) return true;
        }

        return false;
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private List<BlockColor> GetDistinctColors(int n)
    {
        var colors = Enum.GetValues(typeof(BlockColor)).Cast<BlockColor>().ToList();

        // nếu chỉ có ít màu hơn n thì return tất cả (không lặp nếu có thể)
        if (colors.Count <= n)
        {
            // shuffle before returning to keep randomness
            Shuffle(colors);
            return colors;
        }

        // nếu có đủ màu, shuffle và take n
        Shuffle(colors);
        return colors.Take(n).ToList();
    }

    public static bool ShouldUseAdvanced(DataGrid grid)
    {
        int filled = 0;
        int total = grid.Width * grid.Height;

        for (int x = 0; x < grid.Width; x++)
            for (int y = 0; y < grid.Height; y++)
                if (grid.Cells[x, y] != null)
                    filled++;

        float filledRatio = (float)filled / total;

        // Nếu lấp đầy hơn 80% -> gần thua -> ưu tiên dùng advanced để cứu
        if (filledRatio > 0.8f)
            return true;

        // Nếu bất kỳ hàng/cột nào chỉ còn thiếu 1 ô => nên dùng advanced để tạo combo ăn
        for (int y = 0; y < grid.Height; y++)
        {
            int empty = 0;
            for (int x = 0; x < grid.Width; x++)
                if (grid.Cells[x, y] == null) empty++;
            if (empty == 1) return true;
        }

        for (int x = 0; x < grid.Width; x++)
        {
            int empty = 0;
            for (int y = 0; y < grid.Height; y++)
                if (grid.Cells[x, y] == null) empty++;
            if (empty == 1) return true;
        }

        return false;
    }
}

