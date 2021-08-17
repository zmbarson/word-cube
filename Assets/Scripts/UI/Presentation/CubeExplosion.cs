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

public class CubeExplosion : MonoBehaviour
{
    [SerializeField] private float          shakeDuration       = 1f;
    [SerializeField] private float          shakeIntensity      = 0.7f;
    [SerializeField] private int            shakeVibrato        = 1000;
    [SerializeField] private float          explosionForce      = 3000f;
    [SerializeField] private float          explosionRandomness = 1f;
    [SerializeField] private PhysicMaterial physMat;
    [SerializeField] private AudioSource    riser;
    [SerializeField] private AudioSource    explode;

    public YieldInstruction Animate(Cube cube)
    {
        return StartCoroutine(DoExplosionSequence(cube));
    }

    private void ApplyPhysics(Cube cube)
    {
        var bodies = AttachPhysics(cube);
        ApplyExplosionForces(cube.Size * 3f, bodies);
        DeleteUselessComponents(cube);
    }
    private List<Rigidbody> AttachPhysics(Cube cube)
    {
        var bodies = new List<Rigidbody>();
        foreach (var cubit in cube.Cubits)
        {
            var obj       = cubit.gameObject;
            var collision = obj.AddComponent<BoxCollider>();
            var body      = obj.AddComponent<Rigidbody>();
            bodies.Add(body);
            collision.material = physMat;
        }

        return bodies;
    }
    
    private void ApplyExplosionForces(float radius, List<Rigidbody> bodies)
    {
        foreach (var body in bodies)
        {
            body.AddRelativeTorque(Random.Range(0f, explosionRandomness) * Random.onUnitSphere);
            var offset = Random.Range(0, explosionRandomness) * Random.onUnitSphere;
            body.AddExplosionForce(explosionForce, Vector3.zero + offset, radius);
        }
    }

    private void DeleteUselessComponents(Cube cube)
    {
        cube.DestroyChildren<Slice>();
        cube.DestroyChildren<Cubit>();
        cube.DestroyChildren<CubitFace>();
    }

    private IEnumerator DoExplosionSequence(Cube cube)
    {
        yield return StartCoroutine(Warmup(cube));
        ApplyPhysics(cube);
    }

    IEnumerator Warmup(Cube cube)
    {
        yield return new WaitForSeconds(0.2f);

        var deflate = cube.Transform
           .DOScale(Vector3.one * 0.33f, 0.125f)
           .SetEase(Ease.OutCirc);

        var cam = Camera.main
           .DOShakePosition(shakeDuration, shakeIntensity, shakeVibrato, 90, false);

        var spin = cube.Transform
           .DORotate(Random.rotation.eulerAngles * 3f, shakeDuration + 0.125f, RotateMode.FastBeyond360)
           .SetEase(Ease.InCirc);

        var inflate = cube.Transform
           .DOScale(Vector3.one * 1f, 0.125f)
           .SetEase(Ease.InExpo)
           .OnComplete(
                       () =>
                       {
                           StartCoroutine(TweenTimeScale(0.15f, 1f));
                           riser.Stop();
                           explode.Play();
                       }
                      );

        riser.Play();
        yield return DOTween.Sequence()
           .Append(deflate)
           .Append(cam)
           .Insert(shakeDuration, inflate)
           .Insert(0f, spin)
           .Play()
           .WaitForCompletion();
    }

    private IEnumerator TweenTimeScale(float s, float time)
    {
        yield return new WaitForSeconds(0.1f);
        float elapsed = 0;


        Camera.main
           .DOShakePosition(time, shakeIntensity * 0.65f, shakeVibrato, 90, false)
           .Play();

        while (true)
        {
            var ratio = Mathf.Clamp01(elapsed / time);
            ratio          = ratio * ratio * ratio * ratio * ratio;
            Time.timeScale = Mathf.Lerp(s, 1f, ratio);
            if (elapsed > time) break;
            yield return null;
            elapsed += Time.unscaledDeltaTime;
        }
    }
}