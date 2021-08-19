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
        public float[] cameraDistances = { 9f, 11f, 15f };
        public float[] puzzleDensities = { 0.7f, 0.7f, 0.7f };
        public int[]   comboObjectives = { 3, 3, 3 };

        public float GetCameraDistance(PuzzleDifficulty difficulty)
        {
            return cameraDistances[difficulty.ToInt()];
        }

        public float GetDensity(PuzzleDifficulty difficulty)
        {
            return puzzleDensities[difficulty.ToInt()];
        }

        public int GetComboGoal(PuzzleDifficulty difficulty)
        {
            return comboObjectives[difficulty.ToInt()];
        }
    }

    [SerializeField]
    private GameParameters   parameters;
    private GameSession      session;
    private GamePresentation presentation;
    private CubeController   controller;
    private PostGame         postgame;
    private bool             isGameLoaded;

    private void Awake()
    {
        session      = GetComponent<GameSession>();
        presentation = GetComponent<GamePresentation>();
        controller   = GetComponent<CubeController>();
        postgame     = GetComponent<PostGame>();
        ConfigureUI();
    }

    public YieldInstruction StartGame()
    {
        return StartCoroutine(SetupGame());
    }

    private IEnumerator SetupGame()
    {
        yield return App.FadeIn(false);
        yield return presentation.DisplayDifficultyMenu();
        while (!isGameLoaded) yield return null;
        yield return presentation.RevealWord();
        presentation.AllowUIInput(true);
        session.Unpause();
        controller.Enable();
        while (!postgame.IsFinished) yield return null;
        yield return App.FadeOut(true);
    }

    private void ConfigureUI()
    {
        presentation.DifficultyChosen += PrepareGame;

        presentation.WordButtonClicked += () => 
        {
              if (controller.IsAnimating) return;
              presentation.DisplayDictScreen();
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
    }

    private void PrepareGame(PuzzleDifficulty difficulty)
    {
        var dictEntry = WordBank.RandomWord(difficulty);

        Cube   cube   = CubeBuilder.Construct(difficulty.WordLength());
        Puzzle puzzle = new Puzzle(cube, dictEntry.word, parameters.GetDensity(difficulty));

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

        presentation.SetDictionaryInfo(dictEntry.word, dictEntry.partOfSpeech,
                                       dictEntry.pronunciation, dictEntry.definition);

        controller.Possess(cube);
        controller.SetRotation(-17f, 46, -17f);
        App.SetCameraDistance(parameters.GetCameraDistance(difficulty));
        isGameLoaded = true;
    }
}
