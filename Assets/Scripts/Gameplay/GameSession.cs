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
using System.Collections.ObjectModel;
using UnityEngine;

[RequireComponent(typeof(CubeController))]
public class GameSession : MonoBehaviour
{
    public event Action<Cube, float, int, int, ReadOnlyCollection<PuzzleSolution>> PuzzleSolved;
    public event Action<int>                                                       ComboChanged;


    public WordInfo WordInfo        { get; private set; }
    public string   Word            { get; private set; }
    public int      Moves           { get; private set; }
    public int      CurrentCombo    { get; private set; }
    public int      ComboToWin      { get; private set; }
    public float    ElapsedPlayTime { get; private set; }
    public bool     GameOver        { get; private set; }


    private Cube   cube;
    private Puzzle puzzle;
    private bool   timerPaused = true;
    private bool   ready;

    private void Update()
    {
        if (!timerPaused) ElapsedPlayTime += Time.deltaTime;
    }

    public void Setup(Cube cube, WordInfo wordObjective, int comboObjective, Puzzle puzzle)
    {
        if (ready) return;
        WordInfo                    =  wordObjective;
        Word                        =  WordInfo.word;
        ComboToWin                  =  comboObjective;
        this.cube                   =  cube;
        this.puzzle                 =  puzzle;
        puzzle.SolutionCountChanged += CheckGameOver;
        cube.SliceUpdated           += _ => Moves++;
        ready                       =  true;
    }

    public void Pause()
    {
        timerPaused = true;
    }

    public void Unpause()
    {
        timerPaused = false;
    }

    private void CheckGameOver(int newCount)
    {
        if (newCount != CurrentCombo)
        {
            var delta = newCount - CurrentCombo;
            CurrentCombo = newCount;
            ComboChanged?.Invoke(delta);
            if (CurrentCombo >= ComboToWin)
            {
                GameOver    = true;
                timerPaused = true;
                PuzzleSolved?.Invoke(cube, ElapsedPlayTime, Moves, CurrentCombo - ComboToWin, puzzle.Solutions);
            }
        }
    }
}