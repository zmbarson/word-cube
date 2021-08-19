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
using UnityEngine;
using UnityEngine.UI;

public class GamePresentation : MonoBehaviour
{
    public event Action                       SnapButtonClicked;
    public event Action                       WordButtonClicked;
    public event Action<PuzzleDifficulty>     DifficultyChosen;

    [SerializeField] private DifficultyScreen difficultyMenu;
    [SerializeField] private DictionaryScreen dictScreen;
    [SerializeField] private GoalText         wordText;
    [SerializeField] private ComboText        comboText;
    [SerializeField] private Image            cover;
    [SerializeField] private CanvasGroup      inGameUIGroup;
    [SerializeField] private AppButton        wordButton;
    [SerializeField] private AppButton        snapButton;
    [SerializeField] private GameAudio        inGameMusic;

    private void Awake()
    {
        AllowUIInput(false);
        wordButton.OnClick              += () => WordButtonClicked?.Invoke();
        snapButton.OnClick              += () => SnapButtonClicked?.Invoke();
    }

    public YieldInstruction DisplayDifficultyMenu()
    {
        return StartCoroutine(DoDisplayDifficultyMenu());
    }

    public YieldInstruction RevealWord()
    {
        return StartCoroutine(DoRevealWord());
    }


    public void DisplayDictScreen()
    {
        StartCoroutine(OpenDictScreen());
    }

    public void Hide()
    {
        inGameMusic.FadeOut();
        AllowUIInput(false);
        inGameUIGroup
           .DOFade(0f, 0.66f)
           .Play()
           .WaitForCompletion();
    }

    public void AllowUIInput(bool enable)
    {
        inGameUIGroup.interactable   = enable;
        inGameUIGroup.blocksRaycasts = enable;
    }

    public void SetDictionaryInfo(string word, string partOfSpeech, string pronunciation, string definition)
    {
        dictScreen.Set(word, partOfSpeech, pronunciation, definition);
        wordText.SetText(word);
    }

    public void UpdateComboCount(int current, int max, int delta)
    {
        comboText.UpdateCount(current, max, delta);
        inGameMusic.PlayLayers(current);
    }

    private IEnumerator DoDisplayDifficultyMenu()
    {
        yield return difficultyMenu.Display();
        DifficultyChosen?.Invoke(difficultyMenu.Selection.Value);
        Destroy(difficultyMenu.gameObject);
    }

    private IEnumerator DoRevealWord()
    {
        yield return dictScreen.Show(0.4f);
        wordText.Show();
        yield return new AwaitTouchOrTimeout(10f);
        yield return dictScreen.Hide(0.4f);
        FadeOutUIBackground();
        wordText.Minimize();
    }

    private IEnumerator OpenDictScreen()
    {
        AllowUIInput(false);
        wordText.Maximize();
        inGameMusic.ApplyLowpass();

        yield return FadeInUIBackground();
        yield return dictScreen.Show(0.25f);
        yield return new AwaitTouchOrTimeout(10f);

        inGameMusic.RemoveLowpass();
        yield return dictScreen.Hide(0.25f);
        yield return wordText.Minimize();
        yield return FadeOutUIBackground();
        AllowUIInput(true);
    }

    private YieldInstruction FadeOutUIBackground()
    {
        return cover
           .DOFade(0f, 0.4f)
           .SetEase(Ease.InQuad)
           .Play()
           .WaitForCompletion();
    }

    private YieldInstruction FadeInUIBackground()
    {
        return cover
           .DOFade(1f, 0.4f)
           .SetEase(Ease.Linear)
           .Play()
           .WaitForCompletion();
    }
}
