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
using UnityEngine;
using UnityEngine.UI;

public class ApplyGlobalColorTheme : MonoBehaviour
{
    public enum Variation
    {
        Normal,
        Dark,
        VeryDark
    }

    [SerializeField]
    private Variation     variation;
    private Action<Color> apply;

    private bool applyThemeUpdates = true;

    public bool ApplyThemeUpdates
    {
        get => applyThemeUpdates;
        set
        {
            applyThemeUpdates = value;
            if (applyThemeUpdates) UpdateColor(App.ColorScheme);
        }
    }

    private void Awake()
    {
        var render = GetComponent<Renderer>();
        var image  = GetComponent<Image>();
        var text   = GetComponent<Text>();

        if (render != null)
            apply = c =>
                {
                    c.a  = render.material.color.a;
                    render.material.color = c;
                };
        else if (image != null)
            apply = c =>
                {
                    c.a = image.color.a;
                    image.color = c;
                };
        else if (text != null)
            apply = c =>
                {
                    c.a = text.color.a;
                    text.color = c;
                };
    }

    private void OnEnable()
    {
        UpdateColor(App.ColorScheme);
    }

    private Color ApplyVariation(Color color)
    {
        if (variation != Variation.Normal)
        {
            Color.RGBToHSV(color, out var h, out var s, out var v);
            if (variation == Variation.Dark)
                color = Color.HSVToRGB(h, Mathf.Clamp01(1.4f * s), 0.6f * v);
            else if (variation == Variation.VeryDark)
                color = Color.HSVToRGB(h, Mathf.Clamp01(1.4f * s), 0.2f * v);
        }

        return color;
    }

    public void UpdateColor(Color color)
    {
        if (!ApplyThemeUpdates) return;
        color = ApplyVariation(color);
        apply(color);
    }
}