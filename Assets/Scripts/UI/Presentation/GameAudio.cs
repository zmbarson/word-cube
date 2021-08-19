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
using UnityEngine;
using UnityEngine.Audio;

public class GameAudio : MonoBehaviour
{
    private const float defaultFadeTime = 0.5f;

    [SerializeField]
    private AudioMixer mixer;

    [SerializeField] 
    private AudioSource[] layers;
    private float[] volumes;

    private Tween[]            layerTweens;
    private Tween              lowpassTween;
    private Tween              masterVolumeTween;

    private void Awake()
    {
        layerTweens = new Tween[layers.Length];
        volumes     = new float[layers.Length];
        for (int i = 0; i < layers.Length; i++)
        {
            volumes[i]  = layers[i].volume;
            if (i != 0)
            {
                layers[i].volume = 0;
            }
            layers[i].Play();
        }
    }

    public void MuteLayers()
    {
        foreach (var layer in layers) layer.volume = 0;
    }

    public void PlayLayers(int n, float fadeTime = defaultFadeTime)
    {
        n = Mathf.Clamp(n, 0, layers.Length);
        for (var i = 0; i < layers.Length; i++)
        {
            var layer = layers[i];
            var tween = layerTweens[i];
            if (i <= n)
            {
                if (tween == null || !tween.IsComplete())
                {
                    tween.Kill();
                    layerTweens[i] = layer.DOFade(volumes[i], fadeTime).Play();
                }
            }
            else
            {
                if (tween == null || !tween.IsComplete())
                {
                    tween.Kill();
                    layerTweens[i] = layer.DOFade(0f, fadeTime).Play();
                }
            }
        }
    }

    public void CueGameplayMusic()
    {
        for (var i = 0; i < layers.Length; i++)
        {
            layers[i].volume = i == 0 ? defaultFadeTime : 0f;
            layers[i].Stop();
            layers[i].Play();
        }
    }

    private YieldInstruction Fade(float val, float time, int start, int finish)
    {
        var seq = DOTween.Sequence();
        for (var i = start; i < finish; i++)
        {
            var layer = layers[i];
            var tween = layerTweens[i];
            if (tween == null || !tween.IsComplete())
            {
                tween.Kill();
                layerTweens[i] = layer.DOFade(val, time);
                seq.Join(layerTweens[i]);
            }
        }

        return seq.Play().WaitForCompletion();
    }

    public YieldInstruction FadeOut(float time = defaultFadeTime)
    {
        return Fade(0f, time, 0, layers.Length);
    }

    public void ApplyLowpass()
    {
        TweenLowpass(500f, 0.125f);
    }
    public void RemoveLowpass()
    {
        TweenLowpass(22000f, 1.5f);
    }

    void TweenLowpass(float to, float time)
    {
        if (lowpassTween != null && lowpassTween.IsPlaying())
        {
            lowpassTween.Kill();
        }
        mixer.DOSetFloat("lowpassCutoff", to, time).SetEase(Ease.Linear).Play();
    }
}