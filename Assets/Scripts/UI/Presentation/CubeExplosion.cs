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

using System.Linq;
using UnityEngine;
public class CubeExplosion : MonoBehaviour
{
    [SerializeField] private float          shakeDuration           = 2f;
    [SerializeField] private float          warmupShakeIntensity    = 0.25f;
    [SerializeField] private float          scaleDuration           = 0.2f;
    [SerializeField] private float          explosionShakeIntensity = 1.5f;
    [SerializeField] private float          slowdownDuration        = 2f;
    [SerializeField] private float          slowdownMultiplier      = 0.1f;
    [SerializeField] private int            shakeVibrato            = 1000;
    [SerializeField] private float          explosionForceMin       = 2000f;
    [SerializeField] private float          explosionForceVariance  = 2000f;
    [SerializeField] private PhysicMaterial physMat;
    [SerializeField] private AudioSource    riser;
    [SerializeField] private AudioSource    explode;

    public YieldInstruction Animate(Cube cube)
    {
        return StartCoroutine(DoExplosionSequence(cube));
    }

    private IEnumerator DoExplosionSequence(Cube cube)
    {
        DeleteUselessObjects(cube);
        yield return StartCoroutine(Warmup(cube));

        Camera.main.DOShakePosition(shakeDuration, explosionShakeIntensity, shakeVibrato).Play();
        StartCoroutine(ApplyPhysicsAmortized(cube.Cubits.ToArray(), cube.Size));
        StartCoroutine(TweenTimeScale(slowdownMultiplier, slowdownDuration));
    }

    private IEnumerator Warmup(Cube cube)
    {
        yield return new WaitForSeconds(0.2f);

        var deflate = cube.Transform
           .DOScale(Vector3.one * 0.33f, scaleDuration)
           .SetEase(Ease.OutCirc);

        var cam = Camera.main
           .DOShakePosition(shakeDuration + scaleDuration, warmupShakeIntensity, shakeVibrato, 90f, false);

        var spin = cube.Transform
           .DOBlendableRotateBy(Random.rotation.eulerAngles * 15f, shakeDuration + scaleDuration, RotateMode.FastBeyond360)
           .SetEase(Ease.InCirc);
        
        var inflate = cube.Transform
           .DOScale(Vector3.one * 1f, scaleDuration)
           .SetEase(Ease.InExpo)
           .SetDelay(shakeDuration);

        riser.Play();
        yield return DOTween.Sequence()
           .Join(deflate)
           .Append(cam)
           .Join(spin)
           .Join(inflate)
           .Play()
           .WaitForCompletion();
        riser.Stop();
        explode.Play();
    }

    private IEnumerator ApplyPhysicsAmortized(Cubit[] cubits, float radius)
    {

        cubits.Shuffle();
        var startTime = Time.realtimeSinceStartupAsDouble;
        foreach (var cubit in cubits)
        {
            if (Time.realtimeSinceStartupAsDouble - startTime > 0.001)
            {
                yield return null;
                startTime = Time.realtimeSinceStartupAsDouble;
            }
            ApplyForce(radius, cubit);
        }
    }

    private void ApplyForce(float radius, Cubit cubit)
    {
        cubit.Collider.enabled = true;
        cubit.Body.isKinematic = false;
        cubit.Body.AddRelativeTorque(Random.Range(0f, explosionForceVariance) * Random.onUnitSphere, ForceMode.VelocityChange);
        cubit.Body.AddExplosionForce(Random.Range(0f, 1f) * explosionForceVariance + explosionForceMin, Vector3.zero, radius);
    }

    private void DeleteUselessObjects(Cube cube)
    {
        foreach (var cubit in cube.InternalCubits)
        {
            Destroy(cubit.gameObject);
        }
        cube.DestroyChildren<Slice>();
        cube.DestroyChildren<CubitFace>();
    }

    private IEnumerator TweenTimeScale(float s, float time)
    {
        DOTween.defaultTimeScaleIndependent = true;
        yield return DOVirtual.Float(s, 1f, time, val => Time.timeScale = val)
           .SetEase(Ease.InSine)
           .Play()
           .WaitForCompletion();
    }
}