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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Cube : Object3DScriptBase
{
    public event Action<Slice> SliceUpdated;

    private Slice[,]  slices;
    private Cubit[,,] cubits;

    public int     Size   { get; private set; }
    public float   Extent => Size / 2f;
    public Vector3 Center => Transform.position;
    public Cubit this[int col, int row, int depth] => cubits[col, row, depth];
    public Cubit this[int id] => cubits[UnlinearizeCol(id), UnlinearizeRow(id), UnlinearizeDepth(id)];
    public IEnumerable<CubitFace> Faces => Cubits.SelectMany(cubit => cubit.ActiveFaces);

    public IEnumerable<Cubit> Cubits
    {
        get
        {
            foreach (var cubit in cubits)
                if (cubit != null)
                    yield return cubit;
        }
    }

    //public IEnumerable<Slice> Slices
    //{
    //    get
    //    {
    //        foreach (var slice in slices) yield return slice;
    //    }
    //}

    public void Init(int size, Cubit[,,] cubits)
    {
        Size        = size;
        this.cubits = cubits;
        PrepareSlices();
    }

    public Slice[] GetSlicesContaining(Cubit cubit)
    {
        return new[]
        {
            slices[Axis.Forward.ToInt(), cubit.Depth],
            slices[Axis.Right.ToInt(), cubit.Col],
            slices[Axis.Up.ToInt(), cubit.Row]
        };
    }

    public CubeSurface GetSurface(Axis normal)
    {
        var faces = new CubitFace[Size, Size];
        using var enumerator = Cubits
                   .OrderBy(LinearizeIndex)
                   .SelectMany(cubit => cubit.ActiveFaces)
                   .Where(face => face.Transform.forward == Transform.TransformDirection(normal.ToVector()))
                   .GetEnumerator();
        enumerator.MoveNext();
        for (var i = 0; i < Size; i++)
        for (var j = 0; j < Size; j++)
        {
            faces[i, j] = enumerator.Current;
            enumerator.MoveNext();
        }

        return new CubeSurface(faces, normal);
    }

    public IEnumerable<CubeSurface> GetSurfaces(Cube cube)
    {
        yield return GetSurface(Axis.Up);
        yield return GetSurface(Axis.Down);
        yield return GetSurface(Axis.Left);
        yield return GetSurface(Axis.Right);
        yield return GetSurface(Axis.Forward);
        yield return GetSurface(Axis.Back);
    }

    public int LinearizeIndex(Cubit cubit)
    {
        if (cubit == null) return -1;
        return LinearizeIndex(cubit.Col, cubit.Row, cubit.Depth);
    }

    public int LinearizeIndex(int x, int y, int z)
    {
        return x + y * Size + z * Size * Size;
    }

    //public Vector3 UnlinearizeIndex(int index)
    //{
    //    var x = index % Size;
    //    var y = index / Size % Size;
    //    var z = index / (Size * Size);
    //    return new Vector3(x, y, z);
    //}

    public Vector3 NearestCubitIndex(Vector3 point)
    {
        var local        = Transform.InverseTransformPoint(point);
        var centerOffset = Vector3.one * Extent;
        return (local + centerOffset)
               .TruncateComponents()
               .ClampComponents(0, Size - 1);
    }

    public Cubit NearestCubit(Vector3 point)
    {
        var index = NearestCubitIndex(point);
        return this[(int) index.x, (int) index.y, (int) index.z];
    }

    //public Vector3 NearestFaceAxis(Vector3 vec)
    //{
    //    var axes = new[]
    //    {
    //        Transform.forward, -Transform.forward,
    //        Transform.right, -Transform.right,
    //        Transform.up, -Transform.up
    //    };

    //    var   best = Vector3.zero;
    //    float max  = 0;
    //    foreach (var axis in axes)
    //    {
    //        var dot = Vector3.Dot(vec, axis);
    //        if (dot > max)
    //        {
    //            best = axis;
    //            max  = dot;
    //        }
    //    }
    //    return best;
    //}

    public void OnSliceUpdated(Slice moved)
    {
        var cubitsToReIndex = moved.Cubits.ToList();
        cubitsToReIndex.ForEach(c => cubits[c.Col, c.Row, c.Depth] = null);
        cubitsToReIndex.ForEach(c =>
                                {
                                    ReIndex(c);
                                    cubits[c.Col, c.Row, c.Depth] = c;
                                });
        SliceUpdated?.Invoke(moved);
    }

    private void PrepareSlices()
    {
        slices = new Slice[Axis.Count.ToInt(), Size];

        for (var i = 0; i < Size; i++)
        {
            var slice = SliceBuilder.MakeDepth(this, i);
            slices[Axis.Forward.ToInt(), i] = slice;
            slices[Axis.Back.ToInt(), i]    = slice;

            slice = SliceBuilder.MakeCol(this, i);
            slices[Axis.Right.ToInt(), i] = slice;
            slices[Axis.Left.ToInt(), i]  = slice;

            slice = SliceBuilder.MakeRow(this, i);
            slices[Axis.Up.ToInt(), i]   = slice;
            slices[Axis.Down.ToInt(), i] = slice;
        }
    }

    private void ReIndex(Cubit cubit)
    {
        var index = NearestCubitIndex(cubit.Transform.position);
        cubit.SetIndices((int) index.x, (int) index.y, (int) index.z);
    }

    private int UnlinearizeCol(int index)
    {
        return index % Size;
    }

    private int UnlinearizeRow(int index)
    {
        return index / Size % Size;
    }

    private int UnlinearizeDepth(int index)
    {
        return index / (Size * Size);
    }
}