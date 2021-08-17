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
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class App : StaticScriptBase<App>
{
    [SerializeField] 
    private float sceneTransitionDuration = 1f;

    [SerializeField]
    private new    AppCamera camera;
    //private static AppCamera Camera => instance.camera;

    private       Color  colorScheme;
    public static Color  ColorScheme => instance.colorScheme;

    [SerializeField]
    private new AppAudio audio;


    void Start()
    {
        // QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 60;
        StartCoroutine(Main());
    }

    private IEnumerator Main()
    {
        while (Application.isPlaying)
        {
            yield return StartCoroutine(DisplayMainMenu());
            yield return StartCoroutine(Play());
        }
    }

    private IEnumerator DisplayMainMenu()
    {
        RandomizeTheme();
        yield return LoadScene(Scene.MAIN_MENU);
        var menu = FindObjectOfType<MainMenuScreen>();
        yield return menu.Show();
    }

    private IEnumerator Play()
    {
        yield return LoadScene(Scene.GAMEPLAY);
        var setup = FindObjectOfType<GameSetup>();
        yield return setup.StartGame();
    }

    private YieldInstruction LoadScene(int index)
    {
        DOTween.KillAll();
        return SceneManager.LoadSceneAsync(index);
    }

    public static YieldInstruction FadeIn(bool music)
    {
        return instance.StartCoroutine(instance.DoFadeIn(music));
    }

    public static YieldInstruction FadeOut(bool music)
    {
        return instance.StartCoroutine(instance.DoFadeOut(music));
    }

    private IEnumerator DoFadeIn(bool music)
    {
        audio.FadeIn(music ? sceneTransitionDuration : 0f);
        yield return camera.FadeIn(sceneTransitionDuration);
    }

    private IEnumerator DoFadeOut(bool music)
    {
        audio.FadeOut(music ? sceneTransitionDuration : 0f);
        yield return camera.FadeOut(sceneTransitionDuration);

    }

    private void RandomizeTheme()
    {
        colorScheme = Random.ColorHSV(0f, 1f, 0.2f, 0.8f, 1f, 1f);
        var themeable = FindObjectsOfType<ApplyGlobalColorTheme>();
        if (themeable != null)
            foreach (var item in themeable)
                item.UpdateColor(ColorScheme);
        camera.SetBackgroundColor(ColorScheme);
    }
    public static void SetCameraDistance(float d)
    {
        instance.camera.SetCameraDistance(d);
    }

}