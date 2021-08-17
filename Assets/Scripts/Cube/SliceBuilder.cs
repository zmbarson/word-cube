// 
// 
// Copyright (c) 2018-2021 ze_eb
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// 
// 

using System;
using System.Linq;
using UnityEngine;

public static class SliceBuilder
{
    private static readonly Axis COL_AXIS   = Axis.Right;
    private static readonly Axis ROW_AXIS   = Axis.Up;
    private static readonly Axis DEPTH_AXIS = Axis.Forward;

    public static string MakeName(Axis axis, int order)
    {
        return $"Slice {axis.ToVector()} #{order}";
    }

    public static Vector3 CalculateCenter(Axis axis, int order, Cube owner)
    {
        var centerOffset = -axis.ToVector() * (owner.Size - 1) / 2;
        var orderOffset  = axis.ToVector() * order;
        return owner.Center + centerOffset + orderOffset;
    }

    public static Slice MakeCol(Cube owner, int order)
    {
        var container = new GameObject(MakeName(COL_AXIS, order))
        {
            transform =
            {
                position = CalculateCenter(COL_AXIS, order, owner),
                parent   = owner.transform
            }
        };

        var slice = container.AddComponent<Slice>();
        slice.Setup(owner, COL_AXIS, ColIndices(owner, order));
        return slice;
    }

    public static Slice MakeRow(Cube owner, int order)
    {
        var container = new GameObject(MakeName(ROW_AXIS, order))
        {
            transform =
            {
                position = CalculateCenter(ROW_AXIS, order, owner),
                parent   = owner.transform
            }
        };

        var slice = container.AddComponent<Slice>();
        slice.Setup(owner, ROW_AXIS, RowIndices(owner, order));
        return slice;
    }

    public static Slice MakeDepth(Cube owner, int order)
    {
        var container = new GameObject(MakeName(DEPTH_AXIS, order))
        {
            transform =
            {
                position = CalculateCenter(DEPTH_AXIS, order, owner),
                parent   = owner.transform
            }
        };
    
        var slice = container.AddComponent<Slice>();
        slice.Setup(owner, DEPTH_AXIS, DepthIndices(owner, order));
        return slice;
    }

    static int[] IndicesWhere(Cube owner, Func<Cubit, bool> predicate)
    {
        return owner
                .Cubits
                .Where(predicate)
                .Select(owner.LinearizeIndex)
                .ToArray();
    }

    static int[] ColIndices(Cube owner, int col)
    {
        return IndicesWhere(owner, cubit => cubit != null && cubit.Col == col); //only valid for a cube that has never been twisted! (e.g. just made)
    }

    static int[] RowIndices(Cube owner, int row)
    {
        return IndicesWhere(owner, cubit => cubit != null && cubit.Row == row); //only valid for a cube that has never been twisted! (e.g. just made)
    }

    static int[] DepthIndices(Cube owner, int depth)
    {
        return IndicesWhere(owner, cubit => cubit != null && cubit.Depth == depth); //only valid for a cube that has never been twisted! (e.g. just made)
    }
}