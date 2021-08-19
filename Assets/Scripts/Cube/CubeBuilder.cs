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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class CubeBuilder
{
    public static readonly Cubit cubitPrototype = Resources.Load<Cubit>("Prefabs/Cubit");
    public static readonly CubitFace facePrototype = Resources.Load<CubitFace>("Prefabs/CubitFace");

    public static Cube Construct(int size)
    {
        var cube   = MakeParent(size);
        var cubits = BuildCubits(size, cube);
        var slices = MakeSlices(cube, cubits, size);
        cube.Init(size, cubits, slices);
        return cube;
    }

    private static Cubit[,,] BuildCubits(int size, Cube parent)
    {
        var result = new Cubit[size, size, size];
        for (var depth = 0; depth < size; depth++)
        for (var row = 0; row < size; row++)
        for (var col = 0; col < size; col++)
        {
            if (IsInterior(col, row, depth, size)) continue;
            var cubit = MakeCubit(size, row, col, depth);
            cubit.Transform.SetParent(parent.Transform);
            result[cubit.Col, cubit.Row, cubit.Depth] = cubit;
        }
        return result;
    }

    private static Cubit MakeCubit(int cubeSize, int row, int col, int depth)
    {
        var centroid             = Vector3.one * (cubeSize - 1) * 0.5f;
        var cubit                = Object.Instantiate(cubitPrototype);
        cubit.name               = $"{col},{row},{depth}";
        cubit.Transform.position = new Vector3(col, row, depth) - centroid;
        cubit.SetIndices(col, row, depth);
        SetupFaces(cubit, cubeSize);
        return cubit;
    }

    private static void SetupFaces(Cubit cubit, int cubeSize)
    {
        var lowerLimit = 0;
        var upperLimit = cubeSize - 1;
        if (cubit.Col == lowerLimit) MakeFace(cubit, Axis.Left, facePrototype);
        if (cubit.Col == upperLimit) MakeFace(cubit, Axis.Right, facePrototype);
        if (cubit.Row == lowerLimit) MakeFace(cubit, Axis.Down, facePrototype);
        if (cubit.Row == upperLimit) MakeFace(cubit, Axis.Up, facePrototype);
        if (cubit.Depth == lowerLimit) MakeFace(cubit, Axis.Back, facePrototype);
        if (cubit.Depth == upperLimit) MakeFace(cubit, Axis.Forward, facePrototype);
    }

    private static void MakeFace(Cubit cubit, Axis direction, CubitFace template)
    {
        var face = Object.Instantiate(template);
        face.name = $"{direction} Face";
        face.Transform.SetParent(cubit.Transform);
        face.Transform.forward = direction.ToVector();
        face.Transform.localPosition = face.Transform.forward * 0.5f;
        face.Parent = cubit;
        cubit[direction] = face;
    }

    private static bool IsInterior(int x, int y, int z, int size)
    {
        var center = (size - 1f) / 2f;
        var dx = x - center;
        var dy = y - center;
        var dz = z - center;
        return dx * dx + dy * dy + dz * dz < center * center;
    }

    private static Cube MakeParent(int size)
    {
        var parent = new GameObject($"Cube {size}x{size}");
        AttachCollider(parent, size);
        SetupCanvas(parent);
        SetupRigidbody(parent);
        return parent.AddComponent<Cube>();
    }

    private static void AttachCollider(GameObject target, int dimension)
    {
        var collider = target.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = Vector3.one * dimension; // * spacing;
        target.layer = LayerMask.NameToLayer("Cube");
    }

    private static void SetupCanvas(GameObject target)
    {
        var canvas = target.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        var scaler = target.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 100f;
        scaler.referencePixelsPerUnit = 100f;
    }

    private static void SetupRigidbody(GameObject target)
    {
        var body = target.AddComponent<Rigidbody>();
        body.angularDrag = 10;
        body.useGravity = false;
    }

    private static Slice MakeSlice(Cube owner, List<int> indices, int order, Axis axis)
    {
        var centerOffset = -axis.ToVector() * (owner.Size - 1) / 2;
        var orderOffset = axis.ToVector() * order;
        var container = new GameObject($"Slice {axis.ToVector()} #{order}")
        {
            transform =
                {
                    position = owner.Center + centerOffset + orderOffset,
                    parent   = owner.Transform
                }
        };
        var slice = container.AddComponent<Slice>();
        slice.Setup(owner, axis, indices);
        return slice;
    }

    private static Slice[,] MakeSlices(Cube owner, Cubit[,,] cubits, int size)
    {
        var indices = new List<int>[3, size];
        for (var i = 0; i < 3; i++)
        for (var j = 0; j < size; j++)
            indices[i, j] = new List<int>(size);

        for (var depth = 0; depth < size; depth++)
        for (var row = 0; row < size; row++)
        for (var col = 0; col < size; col++)
        {
            if (cubits[col, row, depth] == null) continue;
            var index = col + row * size + depth * size * size;
            indices[Axis.Forward.ToInt(), depth].Add(index);
            indices[Axis.Right.ToInt(), col].Add(index);
            indices[Axis.Up.ToInt(), row].Add(index);
        }

        var slices = new Slice[Axis.Count.ToInt(), size];
        for (var order = 0; order < size; order++)
        {
            var members = indices[Axis.Forward.ToInt(), order];
            var slice   = MakeSlice(owner, members, order, Axis.Forward);
            slices[Axis.Forward.ToInt(), order] = slice;
            slices[Axis.Back.ToInt(), order]    = slice;

            members                           = indices[Axis.Right.ToInt(), order];
            slice                             = MakeSlice(owner, members, order, Axis.Right);
            slices[Axis.Right.ToInt(), order] = slice;
            slices[Axis.Left.ToInt(), order]  = slice;

            members                          = indices[Axis.Up.ToInt(), order];
            slice                            = MakeSlice(owner, members, order, Axis.Up);
            slices[Axis.Up.ToInt(), order]   = slice;
            slices[Axis.Down.ToInt(), order] = slice;
        }

        return slices;
    }
}