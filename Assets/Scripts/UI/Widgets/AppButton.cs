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
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button), typeof(CanvasGroup))]
public class AppButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,  IPointerClickHandler
{
    public event Action OnClick;

    [SerializeField] float easeTime = 0.125f;
    [SerializeField] Ease easing = Ease.OutSine;

    protected CanvasGroup group;
    protected Button button;
    protected Tween hoverAnim;
    protected Tween clickAnim;

    public Button Button => button;

    protected virtual void Awake()
    {
        button = GetComponent<Button>();
        group = GetComponent<CanvasGroup>();
        
        hoverAnim = group
            .DOFade(0.5f, easeTime)
            .SetAutoKill(false)
            .SetEase(easing);
        
        clickAnim = button
            .GetComponent<RectTransform>()
            .DOPunchScale(Vector3.one * 0.2f, 0.25f, 10, 0f)
            .OnComplete(() => {
                button.interactable = true;
                OnClick?.Invoke();
            })
            .SetAutoKill(false);
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        hoverAnim.Restart();
    }

    //Detect when Cursor leaves the GameObject
    public void OnPointerExit(PointerEventData pointerEventData)
    {
        hoverAnim.PlayBackwards();
    }

    //Detect if a click occurs
    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if(!button.interactable) return;
        button.interactable = false;
        hoverAnim.Rewind();
        group.alpha = 1f;
        clickAnim.Restart();
    }
}
