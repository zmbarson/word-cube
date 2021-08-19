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
using System.Linq;
using UnityEngine;

public class Slice : Object3DScriptBase
{
    private Cube      owner;
    private List<int> indices;
    private bool      cubitsAttached;

    public Axis    Axis   { get; private set; }
    public Vector3 Normal => Axis.ToVector();

    public float Rotation
    {
        get
        {
            // Compensate for rounding errors
            var angle = Vector3.Dot(Transform.localRotation.eulerAngles, Normal);
            return Mathf.Abs(angle) > 0.0001f ? angle : 0f;
        }
    }

    public IEnumerable<Cubit> Cubits => indices.Select(index => owner[index]);

    public void Setup(Cube owner, Axis axis, List<int> indices)
    {
        this.owner   = owner;
        Axis         = axis;
        this.indices = indices;
    }

    public void BeginRotation()
    {
        AttachCubits();
    }

    public void FinalizeRotation()
    {
        // We always start at zero bc the transform resets after finalizing. 0 means we snapped back or never moved.
        if (Rotation != 0f) owner.OnSliceUpdated(this);
        DetachCubits();
        Transform.localRotation = Quaternion.identity;
    }

    public void SetAxisRotation(float degrees)
    {
        AttachCubits();
        Transform.localRotation = Quaternion.Euler(Axis.ToVector() * degrees);
    }

    private void AttachCubits()
    {
        if (cubitsAttached) return;
        foreach (var cubit in Cubits) cubit.Transform.SetParent(Transform);
        cubitsAttached = true;
    }

    private void DetachCubits()
    {
        if (!cubitsAttached) return;
        var cubits = GetComponentsInChildren<Cubit>();
        foreach (var cubit in cubits) cubit.Transform.SetParent(owner.Transform);
        cubitsAttached = false;
    }
}