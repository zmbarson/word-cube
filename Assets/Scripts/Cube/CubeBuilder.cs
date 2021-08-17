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

using UnityEngine;
using UnityEngine.UI;

public static class CubeBuilder
{
    private const float CUBIT_EXTENT = 0.5f;

    public static readonly Cubit     cubitTemplate = Resources.Load<Cubit>(ResourceIdentifiers.CUBIT);
    public static readonly CubitFace faceTemplate  = Resources.Load<CubitFace>(ResourceIdentifiers.CUBIT_FACE);

    public static Cube Build(int size)
    {
        var parent = MakeParent(size);
        var cubits = MakeCubits(cubitTemplate, faceTemplate, size);
        foreach (var cubit in cubits)
            if (cubit != null)
                cubit.Transform.SetParent(parent.transform);

        var cube = SetupCube(parent, size, cubits);
        return cube;
    }

    private static Cubit[,,] MakeCubits(Cubit cubitPrototype, CubitFace facePrototype, int size)
    {
        var centroid = Vector3.one * (size - 1) * 0.5f; //= Vector3.one * cubitSize * (cubeSize - 1) * 0.5f;
        var cubits   = new Cubit[size, size, size];
        for (var depth = 0; depth < size; depth++)
        for (var row = 0; row < size; row++)
        for (var col = 0; col < size; col++)
        {
            if (IsInterior(col, row, depth, size)) continue;
            var cubit = Object.Instantiate(cubitPrototype);
            cubit.name = $"{col},{row},{depth}";
            cubit.transform.position = new Vector3(col, row, depth) - centroid;
            cubit.SetIndices(col, row, depth);
            cubits[col, row, depth] = cubit;
            SetupFaces(cubit, facePrototype, size);
        }

        return cubits;
    }

    private static void SetupFaces(Cubit cubit, CubitFace template, int cubeSize)
    {
        var lowerLimit = 0;
        var upperLimit = cubeSize - 1;
        if (cubit.Col == lowerLimit) MakeFace(cubit, Axis.Left, template);
        if (cubit.Col == upperLimit) MakeFace(cubit, Axis.Right, template);
        if (cubit.Row == lowerLimit) MakeFace(cubit, Axis.Down, template);
        if (cubit.Row == upperLimit) MakeFace(cubit, Axis.Up, template);
        if (cubit.Depth == lowerLimit) MakeFace(cubit, Axis.Back, template);
        if (cubit.Depth == upperLimit) MakeFace(cubit, Axis.Forward, template);
    }

    private static void MakeFace(Cubit cubit, Axis direction, CubitFace template)
    {
        var face = Object.Instantiate(template);
        face.name = $"{direction} Face";
        face.transform.SetParent(cubit.Transform);
        face.transform.forward       = direction.ToVector();
        face.transform.localPosition = face.transform.forward * CUBIT_EXTENT;
        face.Parent                  = cubit;
        cubit[direction]             = face;
    }

    private static bool IsInterior(int x, int y, int z, int size)
    {
        var center = (size - 1f) / 2f;
        var dx     = x - center;
        var dy     = y - center;
        var dz     = z - center;
        return dx * dx + dy * dy + dz * dz < center * center;
    }

    private static Cube SetupCube(GameObject target, int size, Cubit[,,] cubits)
    {
        var script = target.AddComponent<Cube>();
        script.Init(size, cubits);
        return script;
    }

    private static GameObject MakeParent(int size)
    {
        var parent = new GameObject($"Cube {size}x{size}");
        AttachCollider(parent, size);
        SetupCanvas(parent);
        SetupRigidbody(parent);
        return parent;
    }

    private static void AttachCollider(GameObject target, int dimension)
    {
        var collider = target.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size      = Vector3.one * dimension; // * spacing;
        target.layer       = LayerMask.NameToLayer("Cube");
    }

    private static void SetupCanvas(GameObject target)
    {
        var canvas = target.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        var scaler = target.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit   = 100f;
        scaler.referencePixelsPerUnit = 100f;
    }

    private static void SetupRigidbody(GameObject target)
    {
        var body = target.AddComponent<Rigidbody>();
        body.angularDrag = 10;
        body.useGravity  = false;
    }
}