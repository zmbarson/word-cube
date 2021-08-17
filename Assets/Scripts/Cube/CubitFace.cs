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
using UnityEngine.UI;

public class CubitFace : Object3DScriptBase
{
    [SerializeField] private Text      letterLabel;
    [SerializeField] private Renderer  backgroundRenderer;
    [SerializeField] private Transform textOrientation;

    private Tween colorTween;
    private bool  wasLinked;
    private bool  isLinked;
    private char  letter;

    public Cubit Parent            { get; set; }
    public bool  IsBlank           => char.IsWhiteSpace(letter);
    public char  Letter            => string.IsNullOrEmpty(letterLabel.text) ? ' ' : letter;
    public int   LetterIndex       { get; private set; }

    public Color BackgroundColor
    {
        get => backgroundRenderer.material.color;
        set => backgroundRenderer.material.color = value;
    }

    public bool IsLinked
    {
        get => isLinked;
        set
        {
            if (isLinked != value)
            {
                wasLinked = isLinked;
                isLinked  = value;
            }
        }
    }

    public void SetLetter(char letter, int index)
    {
        if (!char.IsWhiteSpace(letter))
        {
            this.letter         = char.ToLower(letter);
            LetterIndex         = index;
            BackgroundColor     = Color.white;
            letterLabel.enabled = true;
            letterLabel.text = letter
               .ToString()
               .ToUpper();
        }
        else
        {
            SetBlank();
        }
    }

    public void SetBlank()
    {
        letter      = ' ';
        LetterIndex = -1;
        var color = App.ColorScheme;
        Color.RGBToHSV(color, out var h, out var s, out var v);
        color               = Color.HSVToRGB(h, Mathf.Clamp01(1.4f * s), 0.45f * v);
        BackgroundColor     = color;
        letterLabel.text    = string.Empty;
        letterLabel.enabled = false;
    }

    private void ApplySmoothOrientation()
    {
        var upProjection = Vector3.ProjectOnPlane(Vector3.up, Transform.forward);
        textOrientation.rotation = Quaternion.LookRotation(textOrientation.forward, upProjection);
    }

    //private void ApplyDiscreteOrientation()
    //{
    //    var proj    = Vector3.ProjectOnPlane(Vector3.up, Transform.forward);
    //    var raw     = Quaternion.LookRotation(textOrientation.forward, proj);
    //    var local   = Quaternion.Inverse(raw) * textOrientation.rotation;
    //    var clamped = local.eulerAngles;
    //    clamped.z                = Mathz.NearestAngle(90f, clamped.z);
    //    textOrientation.rotation = textOrientation.rotation * Quaternion.Euler(clamped);
    //}

    public void AnimateColor(Color c)
    {
        if (char.IsWhiteSpace(letter)) return;
        if (colorTween == null || colorTween.IsPlaying())
        {
            colorTween.Kill();
            colorTween = null;
        }

        if (backgroundRenderer.material.color != c)
            colorTween = backgroundRenderer.material
               .DOColor(c, 0.33f)
               .Play();
    }

    private void LateUpdate()
    {
        if (IsLinked != wasLinked)
            AnimateColor(IsLinked ? App.ColorScheme : Color.white);
        wasLinked = IsLinked;
        ApplySmoothOrientation();
    }
}