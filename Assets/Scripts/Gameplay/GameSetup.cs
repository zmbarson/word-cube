using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(GameSession), typeof(CubeController))]
[RequireComponent(typeof(GamePresentation), typeof(PostGame))]
public class GameSetup : MonoBehaviour
{
    [Serializable]
    private class GameParameters
    {
        public float[] cameraDistances = {9f, 11f, 15f};
        public float[] puzzleDensities = { 0.7f, 0.7f, 0.7f };
        public int[]   comboObjectives = { 3, 3, 3 };

        public float GetDensity(PuzzleDifficulty difficulty)
        {
            return puzzleDensities[difficulty.ToInt()];
        }

        public int GetComboGoal(PuzzleDifficulty difficulty)
        {
            return comboObjectives[difficulty.ToInt()];
        }

        public float GetCameraDistance(PuzzleDifficulty difficulty)
        {
            return cameraDistances[difficulty.ToInt()];
        }
    }

    [SerializeField]
    private GameParameters          parameters;
    private GameSession             session;
    private GamePresentation        presentation;
    private PostGame postgame;
    private CubeController          controller;

    private void Awake()
    {
        session      = GetComponent<GameSession>();
        controller   = GetComponent<CubeController>();
        presentation = GetComponent<GamePresentation>();
        postgame     = GetComponent<PostGame>();
        ConfigureUI();
    }

    public YieldInstruction StartGame()
    {
        return StartCoroutine(SetupGame());
    }

    private IEnumerator SetupGame()
    {
        // Game initialization and startup.
        yield return App.FadeIn(false);
        yield return presentation.IntroSequence();
        presentation.AllowUIInput(true);
        session.Unpause();
        controller.Enable();
        while (!postgame.IsFinished) yield return null;
        yield return App.FadeOut(true);
    }

    private void ConfigureUI()
    {
        presentation.DifficultyChosen += difficulty =>
             {
                 App.SetCameraDistance(parameters.GetCameraDistance(difficulty));
                 PrepareGame(difficulty);
                 presentation.SetDictionaryInfo(session.Word, session.WordInfo.partOfSpeech,
                                                session.WordInfo.pronunciation, session.WordInfo.definition);
             };

        IEnumerator AlignCube()
        {
            presentation.AllowUIInput(false);
            session.Pause();
            yield return controller.AlignCube();
            presentation.AllowUIInput(true);
            session.Unpause();
        }

        presentation.SnapButtonClicked += () =>
              {
                  if (controller.IsAnimating) return;
                  presentation.StartCoroutine(AlignCube());
              };

        presentation.WordButtonClicked += () =>
            {
                if (controller.IsAnimating) return;
                presentation.DisplayDictScreen();
            };
    }

    private void PrepareGame(PuzzleDifficulty difficulty)
    {
        var dictEntry = WordBank.RandomWord(difficulty);
        var cube      = FindObjectOfType<Cube>() ?? CubeBuilder.Build(dictEntry.word.Length);
        var puzzle    = new Puzzle(cube, dictEntry.word, parameters.GetDensity(difficulty));

        session.Setup(cube, dictEntry, parameters.GetComboGoal(difficulty), puzzle);
        session.PuzzleSolved += (cube, time, moves, bonus, solutions) =>
            {
                controller.Suspend();
                presentation.Hide();
                postgame.OutroSequence(cube, controller, time, moves, bonus, solutions);
            };
        session.ComboChanged += delta => 
            {
                presentation.UpdateComboCount(session.CurrentCombo, session.ComboToWin, delta);
            };

        controller.Possess(cube);
        controller.SetRotation(-17f, 46, -17f);
    }
}
