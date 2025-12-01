using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BlockShape
{
    [SerializeField] int _width;
    [SerializeField] int _height;
    [SerializeField] bool[] _shape;

    /// <summary>
    /// CTOR
    /// </summary>
    /// <param name="width">The maximum width of the shape</param>
    /// <param name="height">The maximum height of the shape</param>
    public BlockShape(int width, int height)
    {
        _width = width;
        _height = height;
        _shape = new bool[_width * _height];
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="shape">A custom shape</param>
    public BlockShape(bool[,] shape)
    {
        _width = shape.GetLength(0);
        _height = shape.GetLength(1);
        _shape = new bool[_width * _height];
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                _shape[GetIndex(x, y)] = shape[x, y];
            }
        }
    }

    /// <summary>
    /// Returns the width of the shapes bounding box
    /// </summary>
    public int width => _width;

    /// <summary>
    /// Returns the height of the shapes bounding box
    /// </summary>
    public int height => _height;

    /// <summary>
    /// Returns true if given local point is part of this shape
    /// </summary>
    public bool IsPartOfShape(Vector2Int localPoint)
    {
        if (localPoint.x < 0 || localPoint.x >= _width || localPoint.y < 0 || localPoint.y >= _height)
        {
            return false; // outside of shape width/height
        }

        var index = GetIndex(localPoint.x, localPoint.y);
        return _shape[index];
    }

    /*
    Converts X & Y to an index to use with _shape
    */
    private int GetIndex(int x, int y)
    {
        y = (_height - 1) - y;
        return x + _width * y;
    }
}
