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

using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeController : MonoBehaviour
{
    public enum State
    {
        Idle,
        AwaitingDrag,
        DragStarted,
        TurnCube,
        TwistSlice,
        SnapCube,
        Suspended
    }

    [SerializeField] private float normalizedDragThreshold = 0.02f; //percent of screen height in pixels
    [SerializeField] private float dragSpeedSlice          = 8;
    [SerializeField] private float turnSpeedSlice          = 120;
    [SerializeField] private float dragSpeedCube           = 65f;
    [SerializeField] private float twistSnapProximity      = 25f;
    [SerializeField] private Ease  clampEase               = Ease.InOutQuart;
    [SerializeField] private float clampSpeed              = 0.5f;


    public bool IsAnimating => state == State.SnapCube ||
                               state == State.TurnCube ||
                               state == State.TwistSlice;


    private Cube            cube;
    private Rigidbody       body;
    private Camera          pov;
    private State state;
    private bool            allowInput;
    private bool            PointerIsPressed => Input.GetButton(InputCommands.TOUCH);
    private bool            PointerClicked   => Input.GetButtonDown(InputCommands.TOUCH);
    private Vector3         PointerPosition  => Input.mousePosition;

    public void Possess(Cube cube)
    {
        this.cube               = cube;
        body                    = cube.GetComponent<Rigidbody>();
        pov                     = Camera.main;
        allowInput              = true;
        Input.multiTouchEnabled = false;
        StartCoroutine(DoInputLogic());
    }

    public void Enable()
    {
        allowInput = true;
    }

    public void Disable()
    {
        allowInput = false;
    }

    public void Suspend()
    {
        state = State.Suspended;
        Disable();
        StopAllCoroutines();
    }

    public YieldInstruction AlignCube()
    {
        if (IsAnimating) return null;
        return StartCoroutine(ClampCubeToNearest(90f));
    }

    public YieldInstruction FaceCamera(Vector3 axis, float time, bool speedBased)
    {
        if (IsAnimating) return null;
        return StartCoroutine(DoFaceCamera(axis, time, speedBased));
    }

    public void SetRotation(float pitch, float yaw, float roll)
    {
        var rot = Quaternion.Euler(pitch, yaw, roll);
        cube.Transform.rotation = rot;
    }

    private IEnumerator DoInputLogic()
    {
        while (true)
            if (allowInput)
            {
                if (PointerIsPressed)
                {
                    if (PointerClicked && ProjectPointer(out var touchedSlices, out var touchedPoint, out var touchedNormal))
                        yield return StartCoroutine(TurnSlice(touchedSlices, touchedPoint, touchedNormal));
                    else yield return StartCoroutine(TurnCube());
                }
                else
                {
                    state = State.Idle;
                    yield return null;
                }
            }
            else
            {
                yield return null;
            }
    }

    private IEnumerator AwaitDrag()
    {
        var dragPixels = Screen.height * normalizedDragThreshold;
        var dragStart  = PointerPosition;
        state = State.AwaitingDrag;
        while (PointerIsPressed && allowInput)
        {
            Vector2 dragDelta = PointerPosition - dragStart;
            if (dragDelta.sqrMagnitude > dragPixels * dragPixels)
            {
                state = State.DragStarted;
                yield break;
            }

            yield return null;
        }

        state = State.Idle;
    }

    private IEnumerator DoFaceCamera(Vector3 normal, float time, bool speedBased)
    {
        //var fwdLocal = Camera.main.transform.InverseTransformDirection(normal);
        while (IsAnimating) yield return null;

        body.angularVelocity = Vector3.zero;
        state                = State.SnapCube;

        var from = cube.Transform.InverseTransformDirection(normal);
        var to   = Vector3.back;
        var rot  = Quaternion.FromToRotation(from, to);

        yield return cube.Transform
           .DOLocalRotate(rot.eulerAngles, time)
           .SetEase(clampEase)
           .SetSpeedBased(speedBased)
           .Play()
           .WaitForCompletion();

        state = State.Idle;
    }

    private IEnumerator ClampCubeToNearest(float angle)
    {
        while (IsAnimating) yield return null;

        body.angularVelocity = Vector3.zero;
        state                = State.SnapCube;

        var euler        = cube.Transform.eulerAngles;
        var eulerClamped = euler.ApplyToComponents(c => Mathz.NearestAngle(angle, c));

        if (Mathf.Approximately(euler.x, eulerClamped.x) &&
            Mathf.Approximately(euler.y, eulerClamped.y) &&
            Mathf.Approximately(euler.z, eulerClamped.z)) yield break;

        yield return cube.Transform
           .DORotate(eulerClamped, clampSpeed)
           .SetSpeedBased(true)
           .SetEase(clampEase)
           .Play()
           .WaitForCompletion();

        state = State.Idle;
    }

    private IEnumerator TurnCube()
    {
        yield return StartCoroutine(AwaitDrag());
        state = State.TurnCube;
        var lastPosition = PointerPosition;
        while (PointerIsPressed && allowInput)
        {
            var delta = PointerPosition - lastPosition;
            lastPosition = PointerPosition;
            var dragDirection = new Vector3(delta.y, -delta.x); //.normalized;
            var axis          = cube.Transform.InverseTransformDirection(dragDirection);
            var force         = delta.magnitude * dragSpeedCube;
            body.AddRelativeTorque(axis * force, ForceMode.Force);
            yield return null;
            // yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator TurnSlice(IEnumerable<Slice> touched, Vector3 touchPosition, Vector3 touchNormal)
    {
        yield return StartCoroutine(AwaitDrag());

        if (state == State.Idle || !allowInput) yield break;
        state = State.TwistSlice;

        var facePlane       = new Plane(touchNormal, touchPosition);
        var relativeNormal  = cube.Transform.InverseTransformDirection(touchNormal);
        var dragProjected   = ProjectPointer(facePlane) - touchPosition;
        var sliceAxisApprox = Vector3.Cross(facePlane.normal, dragProjected.normalized);
        var slice           = SelectBestAligned(touched, sliceAxisApprox);
        var originalAngle   = slice.Rotation;
        var angle           = originalAngle;
        var angleMin        = Mathz.NearestAngle(90f, angle - 90f); //-90
        var angleMax        = Mathz.NearestAngle(90f, angle + 90f); //0

        while (PointerIsPressed && allowInput)
        {
            dragProjected = ProjectPointer(facePlane) - touchPosition;
            var dragRelative = cube.Transform.InverseTransformDirection(dragProjected);
            var axis         = Vector3.Cross(slice.Normal, relativeNormal).normalized;
            angle = Vector3.Dot(axis, dragRelative) * dragSpeedSlice;
            angle = Mathf.Clamp(angle, angleMin, angleMax);

            // Cancels the drag as soon as the slice is rotated +/- degrees
            // Without this you can freely rotate the slice through its possible 180deg
            // range of motion until you let go of it.
            var nearest = Mathz.NearestAngle(90f + twistSnapProximity, angle);
            if (!Mathf.Approximately(nearest, originalAngle))
            {
                yield return AwaitSliceSnap(slice, angle);
                yield break;
            }

            slice.SetAxisRotation(angle);
            yield return null;
        }

        // The slice was released before reacing the snap threshold
        if (!Mathf.Approximately(angle, angleMax) &&
            !Mathf.Approximately(angle, angleMin))
            yield return StartCoroutine(AwaitSliceSnap(slice, angle));
        //while (PointerIsPressed) yield return null;
    }

    private IEnumerator AwaitSliceSnap(Slice slice, float intendedAngle)
    {
        intendedAngle = Mathz.NearestAngle(90f, intendedAngle);
        var delta    = Mathf.DeltaAngle(slice.Rotation, intendedAngle);
        var duration = Mathf.Abs(delta) / turnSpeedSlice;

        slice.BeginRotation();
        yield return slice
           .Transform
           .DOLocalRotateQuaternion(Quaternion.Euler(slice.Normal * delta), duration)
           .SetRelative()
           .SetEase(Ease.OutSine)
           .Play()
           .WaitForCompletion();
        slice.FinalizeRotation();
    }

    private Ray PointerRay()
    {
        return pov.ScreenPointToRay(PointerPosition);
    }

    private bool ProjectPointer(out Slice[] slices, out Vector3 point, out Vector3 normal)
    {
        if (Physics.Raycast(PointerRay(), out var intersection, 2000f, CollisionLayers.CUBE_LAYERMASK))
        {
            point  = intersection.point;
            normal = intersection.normal;
            slices = cube.GetSlicesContaining(cube.NearestCubit(point));
            return true;
        }

        slices = null;
        point  = Vector3.zero;
        normal = Vector3.zero;
        return false;
    }

    private Vector3 ProjectPointer(Plane plane)
    {
        var pointer = PointerRay();
        plane.Raycast(pointer, out var enter);
        return pointer.GetPoint(enter);
    }

    private static Slice SelectBestAligned(IEnumerable<Slice> slices, Vector3 axis)
    {
        Slice slice         = null;
        float maxProjection = 0;
        foreach (var item in slices)
        {
            var worldAxis  = item.Transform.TransformDirection(item.Normal);
            var dot        = Vector3.Dot(worldAxis, axis);
            var projection = Mathf.Abs(dot);
            if (projection > maxProjection)
            {
                slice         = item;
                maxProjection = projection;
            }
        }

        return slice;
    }
}