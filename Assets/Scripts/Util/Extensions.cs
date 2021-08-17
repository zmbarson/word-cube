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
using System.Collections.ObjectModel;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public static class AxisExtensions
{
    private static readonly ReadOnlyCollection<Axis> OrthogonalToX =
        new ReadOnlyCollection<Axis>(new[] {Axis.Forward, Axis.Back, Axis.Up, Axis.Down});

    private static readonly ReadOnlyCollection<Axis> OrthogonalToY =
        new ReadOnlyCollection<Axis>(new[] {Axis.Forward, Axis.Back, Axis.Right, Axis.Left});

    private static readonly ReadOnlyCollection<Axis> OrthogonalToZ =
        new ReadOnlyCollection<Axis>(new[] {Axis.Right, Axis.Left, Axis.Up, Axis.Down});

    public static int ToInt(this Axis axis)
    {
        return (int) axis;
    }

    public static Vector3 ToVector(this Axis axis)
    {
        switch (axis)
        {
            case Axis.Forward: return Vector3.forward;
            case Axis.Back: return Vector3.back;
            case Axis.Right: return Vector3.right;
            case Axis.Left: return Vector3.left;
            case Axis.Up: return Vector3.up;
            case Axis.Down: return Vector3.down;
        }

        return Vector3.zero;
    }

    public static ReadOnlyCollection<Axis> Orthogonal(this Axis axis)
    {
        switch (axis)
        {
            case Axis.Left:
            case Axis.Right:
                return OrthogonalToX;
            case Axis.Up:
            case Axis.Down:
                return OrthogonalToY;
            case Axis.Forward:
            case Axis.Back:
                return OrthogonalToZ;
            default:
                return null;
        }
    }
}

public static class PuzzleDifficultyExtensions
{
    public static int ToInt(this PuzzleDifficulty difficulty)
    {
        return (int) difficulty;
    }
}

public static class Vector3Extensions
{
    public static Vector3 TruncateComponents(this Vector3 vector)
    {
        //return ApplyToComponents(vector, Mathf.Floor);
        var x = Mathf.Floor(vector.x);
        var y = Mathf.Floor(vector.y);
        var z = Mathf.Floor(vector.z);
        return new Vector3(x, y, z);
    }

    public static Vector3 ClampComponents(this Vector3 vector, float min, float max)
    {
        var x = Mathf.Clamp(vector.x, min, max);
        var y = Mathf.Clamp(vector.y, min, max);
        var z = Mathf.Clamp(vector.z, min, max);
        return new Vector3(x, y, z);
    }

    public static Vector3 ApplyToComponents(this Vector3 vector, Func<float, float> transformation)
    {
        var x = transformation(vector.x);
        var y = transformation(vector.y);
        var z = transformation(vector.z);
        return new Vector3(x, y, z);
    }
}

public static class CollectionExtensions
{
    public static IList<T> Shuffle<T>(this IList<T> list, int iterations = 1)
    {
        for (var iter = 0; iter < iterations; iter++)
        for (var i = list.Count - 1; i >= 0; i--)
        {
            var k = Random.Range(0, i + 1);
            (list[k], list[i]) = (list[i], list[k]);
        }

        return list;
    }

    public static T RandomItem<T>(this IList<T> list)
    {
        return list[Random.Range(0, list.Count)];
    }

    public static T First<T>(this IList<T> list)
    {
        return list[0];
    }
}

public static class ComponentExtensions
{
    public static void DestroyChildren<T>(this Component target) where T : UnityEngine.Object
    {
        var objects = target.GetComponentsInChildren<T>();
        if (objects != null)
            foreach (var obj in objects)
                Object.Destroy(obj);
    }
}