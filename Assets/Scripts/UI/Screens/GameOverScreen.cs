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
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour
{
    [SerializeField] private float        displayTime = 10f;
    [SerializeField] private List<string> mainMessages;
    [SerializeField] private List<string> subMessages;
    [SerializeField] private Text         mainLabel;
    [SerializeField] private Text         subLabel;
    [SerializeField] private Text         timeLabel;
    [SerializeField] private Image        timeIcon;
    [SerializeField] private Text         instructionLabel;
    [SerializeField] private Image        background;
    [SerializeField] private float        textFadeTime       = 0.33f;
    [SerializeField] private float        delay              = 0.25f;
    [SerializeField] private float        backgroundFadeTime = 0.25f;
    [SerializeField] private float        backgroundAlpha    = 0.78f;
    [SerializeField] private AudioSource  music;

    private void Awake()
    {
        mainLabel.text = mainMessages.RandomItem().ToUpper();
        subLabel.text  = subMessages.RandomItem().ToUpper();
    }

    public YieldInstruction Show(Cube cube, float time, int moves, int bonus)
    {
        var baseText  = $"{ParseTime(time)} • {moves} MOVES";
        var bonusText = bonus > 0 ? $"• BONUS +{bonus}!" : string.Empty;
        timeLabel.text = $"{baseText} {bonusText}";
        return StartCoroutine(DoShow(cube, time, moves));
    }

    private IEnumerator DoShow(Cube cube, float time, int moves)
    {
        // Do the cube explosion animation.
        // Game over sequence
        music.Play();
        yield return new WaitForSeconds(1f);

        yield return background
           .DOFade(backgroundAlpha, backgroundFadeTime)
           .SetEase(Ease.Linear)
           .Play()
           .WaitForCompletion();

        yield return DOTween.Sequence()
           .Append(mainLabel.DOFade(1f, textFadeTime))
           .Insert(delay, timeLabel.GetComponentInParent<CanvasGroup>().DOFade(1f, textFadeTime))
           .Insert(delay * 2f, subLabel.DOFade(1f, textFadeTime))
           .Insert(delay * 3f, instructionLabel.DOFade(1f, textFadeTime))
           .OnComplete(() => StartCoroutine(AnimateHint()))
           .Play()
           .WaitForCompletion();
        yield return new AwaitTouchOrTimeout(displayTime);
    }

    private IEnumerator AnimateHint()
    {
        yield return new WaitForSeconds(1f);
        instructionLabel
            .DOFade(0f, 2f)
            .SetLoops(-1, LoopType.Yoyo)
            .Play();
    }

    private string ParseTime(float time)
    {
        var min = (int) (time / 60f);
        var sec = Mathf.RoundToInt(time - 60f * min);
        sec = (int) Mathf.Max(0f, sec);
        return $"{min:D2}:{sec:D2}";
    }
}