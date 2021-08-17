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

public class AppAudio : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private string     masterVolParam = "masterVol";
    [SerializeField] private Ease       fadeInEase     = Ease.Linear;
    [SerializeField] private Ease       fadeOutEase    = Ease.Linear;

    private Tween fader;

    public YieldInstruction FadeIn(float duration)
    {
        if (fader != null) fader.Kill(true);
        if (duration <= 0)
        {
            mixer.SetFloat(masterVolParam, 0f);
            return null;
        }

        fader = mixer.DOSetFloat(masterVolParam, 0f, duration)
           .SetEase(fadeInEase)
           .Play();
        return fader.WaitForCompletion();
    }

    public YieldInstruction FadeOut(float duration)
    {
        if (fader != null) fader.Kill(true);
        if (duration <= 0)
        {
            mixer.SetFloat(masterVolParam, -80f);
            return null;
        }

        fader = mixer.DOSetFloat(masterVolParam, -80.0f, duration)
           .SetEase(fadeOutEase)
           .Play();
        return fader.WaitForCompletion();
    }
}