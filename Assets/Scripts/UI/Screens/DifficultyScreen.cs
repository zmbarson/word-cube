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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DifficultyScreen : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private AppButton   easyButton;
    [SerializeField] private AppButton   normalButton;
    [SerializeField] private AppButton   hardButton;
    [SerializeField] private Text        instructions;
    [SerializeField] private float       fadeTime  = 0.25f;
    [SerializeField] private float       fadeDelay = 0.125f;

    public  PuzzleDifficulty? Selection { get; private set; }
    private Tween             outro;

    private void Awake()
    {
        canvasGroup.interactable   = false;
        canvasGroup.blocksRaycasts = false;

        easyButton.GetComponent<CanvasGroup>().alpha = 0f;
        easyButton.OnClick += OnClicked(PuzzleDifficulty.Easy, easyButton, normalButton, hardButton);

        normalButton.GetComponent<CanvasGroup>().alpha = 0f;
        normalButton.OnClick += OnClicked(PuzzleDifficulty.Normal, normalButton, easyButton, hardButton);

        hardButton.GetComponent<CanvasGroup>().alpha = 0f;
        hardButton.OnClick += OnClicked(PuzzleDifficulty.Hard, hardButton, normalButton, easyButton);
    }

    private Action OnClicked(PuzzleDifficulty difficulty, AppButton clicked, params AppButton[] others)
    {
        return () =>
               {
                   Selection                  = difficulty;
                   canvasGroup.blocksRaycasts = false;
                   canvasGroup.interactable   = false;

                   var sequence = DOTween.Sequence();

                   foreach (var button in others)
                       sequence
                          .Join(button.Button
                                   .GetComponent<RectTransform>()
                                   .DOAnchorPosX(-Screen.width / 2f, 0.25f)
                                   .SetRelative(true)
                                   .SetEase(Ease.InOutSine))
                          .Join(button.Button
                                   .GetComponent<CanvasGroup>()
                                   .DOFade(0f, 0.15f)
                                   .SetEase(Ease.InOutSine));
                   sequence.AppendInterval(0.2f);
                   outro = sequence;
               };
    }

    public YieldInstruction Display()
    {
        return StartCoroutine(DisplayMenu());
    }

    private IEnumerator DisplayMenu()
    {
        yield return AnimateIntro();

        canvasGroup.interactable   = true;
        canvasGroup.blocksRaycasts = true;
        while (Selection == null) yield return null;
        yield return AnimateOutro();
    }

    private YieldInstruction AnimateIntro()
    {
        var seq  = DOTween.Sequence();
        var item = 1;
        foreach (var button in Buttons())
        {
            seq.Insert(fadeDelay * item, button
                          .GetComponent<CanvasGroup>()
                          .DOFade(1f, fadeTime));
            item++;
        }

        return seq.Insert(fadeDelay * item, instructions.DOFade(1f, fadeTime))
           .OnComplete(() => StartCoroutine(AnimateHint()))
           .Play().WaitForCompletion();
    }

    private YieldInstruction AnimateOutro()
    {
        return DOTween.Sequence()
           .Append(outro)
           .Append(canvasGroup.DOFade(0f, 0.33f))
           .Play().WaitForCompletion();
    }

    private IEnumerator AnimateHint()
    {
        yield return new WaitForSeconds(1f);
        if (Selection != null) yield break;
        var instructionAnimation = instructions
           .DOFade(0f, 2f)
           .SetLoops(-1, LoopType.Yoyo)
           .Play();

        while (Selection == null) yield return null;

        instructionAnimation.Pause();
        instructionAnimation.Complete();
    }

    private IEnumerable<AppButton> Buttons()
    {
        yield return easyButton;
        yield return normalButton;
        yield return hardButton;
    }
}
