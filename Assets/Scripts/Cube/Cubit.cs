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

[RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
public class Cubit : Object3DScriptBase
{
    private readonly CubitFace[] faces = new CubitFace[Axis.Count.ToInt()];
    public int Col { get; private set; }
    public int Row { get; private set; }
    public int Depth { get; private set; }

    public Rigidbody   Body     { get; private set; }
    public BoxCollider Collider { get; private set; }

    private void Awake()
    {
        Body     = GetComponent<Rigidbody>();
        Collider = GetComponent<BoxCollider>();
    }

    public CubitFace this[Axis axis]
    {
        get => faces[axis.ToInt()];
        set => faces[axis.ToInt()] = value;
    }

    public IEnumerable<CubitFace> ActiveFaces
    {
        get
        {
            foreach (var face in faces)
                if (face != null)
                    yield return face;
        }
    }

    public void SetIndices(int col, int row, int depth)
    {
        Col = col;
        Row = row;
        Depth = depth;
    }
}