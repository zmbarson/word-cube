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

public class MainMenuScreen : MonoBehaviour
{
    private static readonly char[] TitleLetters = {'W', 'O', 'R', 'D', ' ', 'C', 'U', 'B', 'E'};

    [SerializeField] private float           cameraOffset;
    [SerializeField] private RectTransform   textContainer;
    [SerializeField] private Vector2         textOffset;
    [SerializeField] private float           textOffsetTime;
    [SerializeField] private Ease            textOffsetEase;
    [SerializeField] private Text            instructionText;
    [SerializeField] private float           textFadePeriod;
    [SerializeField] private Ease            textFadeEase;
    [SerializeField] private Font            titleFont;
    [SerializeField] private List<Text>      titleText;
    [SerializeField] private List<CubitFace> randomLetters;

    private void Awake()
    {
        PopulateBackground();
    }

    public YieldInstruction Show()
    {
        return StartCoroutine(DoShow());
    }

    private IEnumerator DoShow()
    {
        App.SetCameraDistance(cameraOffset);
        AnimateTitleSequence();
        yield return App.FadeIn(false, 1f);
        yield return new AwaitTouch();
        yield return App.FadeOut(true);
    }

    private void PopulateBackground()
    {
        foreach (var letter in randomLetters)
            letter.SetLetter(TitleLetters.RandomItem(), -1);
    }

    private void AnimateTitleSequence()
    {
        // Animate the bottom text (tap to play)
        var textTween = instructionText
           .DOFade(0f, textFadePeriod)
           .SetLoops(-1, LoopType.Yoyo)
           .SetEase(textFadeEase);

        var textOffsetTween = textContainer
           .DOMove(textOffset, textOffsetTime)
           .SetEase(textOffsetEase)
           .SetRelative()
           .OnComplete(() => textTween.Play());

        // Animate the logo
        var textSeq = DOTween.Sequence();
        titleText.Shuffle();
        for (var i = 0; i < titleText.Count; i++)
        {
            var letter       = titleText[i];
            var originalText = letter.text;
            var j            = 0;
            var tween = DOVirtual
               .Float(0f, 1f, 2 + (i + 1) * 0.25f, f =>
                  {
                      var k = (int) (f / 0.01f);
                      if (k > j)
                      {
                          j           = k;
                          letter.text = RandomLetter();
                      }
                  })
               .OnComplete(() =>
                   {
                       letter.text = originalText;
                       letter.font = titleFont;
                   })
               .SetEase(Ease.Linear)
               .Pause();
            textSeq.Insert(0, tween);
        }

        textSeq
           .AppendCallback(() => textOffsetTween.Play())
           .Play();
    }

    private static string RandomLetter()
    {
        var num = Random.Range(0, 26);
        var let = (char) ('A' + num);
        return let.ToString();
    }
}