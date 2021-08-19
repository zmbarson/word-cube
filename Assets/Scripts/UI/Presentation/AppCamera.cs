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

[RequireComponent(typeof(Camera))]
public class AppCamera : Object3DScriptBase
{
    [SerializeField] private CanvasGroup cover;
    [SerializeField] private Ease        fadeInEase  = Ease.OutSine;
    [SerializeField] private Ease        fadeOutEase = Ease.InSine;

    new private Camera camera;

    private void Awake()
    {
        camera = GetComponent<Camera>();
        cover
           .GetComponent<ApplyGlobalColorTheme>()
           .ApplyThemeUpdates = false;
    }

    public YieldInstruction FadeIn(float duration)
    {
        if (duration <= 0) cover.alpha = 1f;
        return cover
           .DOFade(0f, duration)
           .SetEase(fadeInEase)
           .OnComplete(() =>
               {
                   cover
                      .gameObject
                      .GetComponent<ApplyGlobalColorTheme>()
                      .ApplyThemeUpdates = true;
               })
           .Play()
           .WaitForCompletion();
    }

    public YieldInstruction FadeOut(float duration)
    {
        //print("out");
        return cover
           .DOFade(1f, duration)
           .SetEase(fadeOutEase)
           .OnComplete(() =>
               {
                   cover
                      .gameObject
                      .GetComponent<ApplyGlobalColorTheme>()
                      .ApplyThemeUpdates = false;
               })
           .Play()
           .WaitForCompletion();
    }

    public void SetCameraDistance(float d)
    {
        Transform.position = Vector3.back * d;
    }

    public void SetBackgroundColor(Color color)
    {
        camera.backgroundColor = color;
    }
}