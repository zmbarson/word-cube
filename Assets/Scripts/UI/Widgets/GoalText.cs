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
using UnityEngine;
using UnityEngine.UI;

public class GoalText : MonoBehaviour
{
    [SerializeField] private float         animLength   = 0.4f;
    [SerializeField] private Ease          positionEase = Ease.OutExpo;
    [SerializeField] private Ease          scaleEase    = Ease.OutExpo;
    [SerializeField] private Button        button;
    [SerializeField] private Text          label;
    [SerializeField] private RectTransform root;
    [SerializeField] private RectTransform target;

    private void Awake()
    {
        label.enabled = false;
    }

    public void Show()
    {
        label.enabled = true;
    }

    public void SetText(string word)
    {
        label.text  = word.ToUpper();
    }

    public YieldInstruction Minimize()
    {
        return StartCoroutine(AnimateMinimize());
    }

    private IEnumerator AnimateMinimize()
    {
        yield return DOTween.Sequence()
            .Join(root
                .DOScale(0.5f, animLength)
                .SetEase(scaleEase))
            .Join(button
                .GetComponent<RectTransform>()
                .DOMove(root.position, animLength)
                .SetEase(positionEase))
           .Play()
            .WaitForCompletion();
    }

    public YieldInstruction Maximize()
    {
        return StartCoroutine(AnimateMaximize());
    }

    private IEnumerator AnimateMaximize()
    {
        yield return DOTween.Sequence()
            .Join(root
                .DOScale(1f, animLength)
                .SetEase(scaleEase))
            .Join(button
                .GetComponent<RectTransform>()
                .DOMove(target.position, animLength)
                .SetEase(positionEase))
           .Play()
           .WaitForCompletion();
    }
}