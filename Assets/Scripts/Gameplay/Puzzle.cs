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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Puzzle
{
    public event Action<int> SolutionCountChanged;

    private Cube                   cube;
    private string                 goal;
    private int[][]                duplicateLetters;
    private List<PuzzleSolution>[] solutions;
    private int                    SolutionCount => solutions.Select(s => s.Count).Sum();

    public ReadOnlyCollection<PuzzleSolution> Solutions
    {
        get
        {
            var list = new List<PuzzleSolution>();
            foreach (var set in solutions)
            foreach (var solution in set)
            {
                var letters = solution.members.ToArray();
                if (letters.First().Letter != goal[0]) Array.Reverse(letters);
                list.Add(solution);
            }
            return list.AsReadOnly();
        }
    }

    public Puzzle(Cube cube, string word, float density)
    {
        this.cube         =  cube;
        goal              =  word;
        cube.SliceUpdated += UpdatePuzzleState;
        solutions         =  new List<PuzzleSolution>[6];
        for (var face = 0; face < 6; face++)
            solutions[face] = new List<PuzzleSolution>(2 * 2 * word.Length); // Max number of  suolutions per face for a palindrome.
        duplicateLetters = new int[goal.Length][];
        for (var i = 0; i < goal.Length; i++)
        {
            var indices = new List<int>();
            for (var j = 0; j < goal.Length; j++)
            {
                if (i == j) continue;
                if (goal[i] == goal[j]) indices.Add(j);
            }

            duplicateLetters[i] = indices.ToArray();
        }
        Generate(density);
    }

    #region Resolution

    private void UpdatePuzzleState(Slice sliceMoved)
    {
        var prevCount    = SolutionCount;
        var affectedAxes = sliceMoved.Axis.Orthogonal();
        foreach (var axis in affectedAxes)
        {
            solutions[axis.ToInt()].Clear();
            var surface = cube.GetSurface(axis);
            UpdateLinks(surface);
            ResolveHorizontalSolutions(surface);
            ResolveVerticalSolutions(surface);
        }

        if (SolutionCount != prevCount) SolutionCountChanged?.Invoke(SolutionCount);
    }

    private void UpdateLinks(CubeSurface face)
    {
        for (var row = 0; row < goal.Length; row++)
        for (var col = 0; col < goal.Length; col++)
        {
            if (face[row, col].IsBlank) continue;
            face[row, col].IsLinked =
                CheckLinkAbove(face, row, col, face[row, col].LetterIndex, true) ||
                CheckLinkBelow(face, row, col, face[row, col].LetterIndex, true) ||
                CheckLinkLeft(face, row, col, face[row, col].LetterIndex, true) ||
                CheckLinkRight(face, row, col, face[row, col].LetterIndex, true);
        }
    }

    private void ResolveHorizontalSolutions(CubeSurface face)
    {
        for (var row = 0; row < goal.Length; row++)
        {
            var foundForward  = true;
            var foundBackward = true;
            for (var col = 0; col < goal.Length && (foundForward || foundBackward); col++)
            {
                if (goal[col] != face[row, col].Letter) foundForward                    = false;
                if (goal[goal.Length - 1 - col] != face[row, col].Letter) foundBackward = false;
            }

            if (foundForward || foundBackward)
            {
                var solution = new CubitFace[goal.Length];
                for (var i = 0; i < goal.Length; i++) solution[i] = face[row, i];
                if (foundBackward) Array.Reverse(solution);
                solutions[face.axis.ToInt()].Add(new PuzzleSolution(face, solution));
            }
        }
    }

    private void ResolveVerticalSolutions(CubeSurface face)
    {
        for (var col = 0; col < goal.Length; col++)
        {
            var foundForward  = true;
            var foundBackward = true;
            for (var row = 0; row < goal.Length && (foundForward || foundBackward); row++)
            {
                if (goal[row] != face[row, col].Letter) foundForward                    = false;
                if (goal[goal.Length - 1 - row] != face[row, col].Letter) foundBackward = false;
            }

            if (foundForward || foundBackward)
            {
                var solution = new CubitFace[goal.Length];
                for (var i = 0; i < goal.Length; i++) solution[i] = face[i, col];
                if (foundBackward) Array.Reverse(solution);
                solutions[face.axis.ToInt()].Add(new PuzzleSolution(face, solution));
            }
        }
    }

    #endregion

    #region Construction

    private int[] AllPossiblePlacementsAt(int row, int col)
    {
        return new[]
        {
            row, goal.Length - 1 - row,
            col, goal.Length - 1 - col
        };
    }

    private void Generate(float density)
    {
        var idealBlanksPerFace = Mathf.Max(0 /*1?*/, Mathf.FloorToInt((1 - Mathf.Clamp01(density)) * goal.Length * goal.Length));


        // Used to randomize cell population order
        var indices = new int[goal.Length * goal.Length];
        for (var i = 0; i < goal.Length * goal.Length; i++)
            indices[i] = i;

        // TODO: change implementation to verify the puzzle is solvable.
        // The current approach sometimes produces unsolvable puzzles.
        // This is most obvious with the 3x3 cube.
        // Naive approach: ensure each letter is placed 3 timers (once per solution) before placing others.
        // THIS IS NOT THE PROPER WAY TO DO IT, but it helps with the 3x3 cube in most cases.
        var counts = new int[goal.Length];
        var placedCount = 0;
        //-----------------------------

        var faces     = cube.GetSurfaces(cube);
        foreach (var grid in faces)
        {
            var numBlanks = 0;
            indices.Shuffle();
            foreach (var index in indices)
            {
                var row           = index / goal.Length;
                var col           = index % goal.Length;
                var possibilities = AllPossiblePlacementsAt(row, col).Shuffle();
                var placed        = false;
                foreach (var possibility in possibilities)
                {
                    // TODO: remove when real solution verification is implemented.
                    if (placedCount < goal.Length * 3 && counts[possibility] >= 3)
                    {
                        continue;
                    }
                    //-----------------------------

                    if (!CheckLinkAbove(grid, row, col, possibility, true) &&
                        !CheckLinkBelow(grid, row, col, possibility, true) &&
                        !CheckLinkLeft(grid, row, col, possibility, true) &&
                        !CheckLinkRight(grid, row, col, possibility, true))
                    {
                        grid[row, col].SetLetter(goal[possibility], possibility);
                        placed = true;
                        counts[possibility]++;
                        placedCount++;
                        break;
                    }
                }

            if (!placed)
                {
                    grid[row, col].SetBlank();
                    numBlanks++;
                }
            }

            while (numBlanks < idealBlanksPerFace)
            {
                var row = Random.Range(0, goal.Length);
                var col = Random.Range(0, goal.Length);
                grid[row, col].SetBlank();
                numBlanks++;
            }
        }
    }

    #endregion

    #region Query / utility

    private bool AdjacentAbove(CubeSurface face, int row, int col, out CubitFace neighbor)
    {
        var exists = row + 1 < face.size;
        neighbor = exists ? face[row + 1, col] : null;
        return exists;
    }

    private bool AdjacentBelow(CubeSurface face, int row, int col, out CubitFace neighbor)
    {
        var exists = row - 1 >= 0;
        neighbor = exists ? face[row - 1, col] : null;
        return exists;
    }

    private bool AdjacentLeft(CubeSurface face, int row, int col, out CubitFace neighbor)
    {
        var exists = col - 1 >= 0;
        neighbor = exists ? face[row, col - 1] : null;
        return exists;
    }

    private bool AdjacentRight(CubeSurface face, int row, int col, out CubitFace neighbor)
    {
        var exists = col + 1 < face.size;
        neighbor = exists ? face[row, col + 1] : null;
        return exists;
    }

    private bool CheckLinkAbove(CubeSurface face, int row, int col, int letter, bool searchAliases)
    {
        var prevLetterExists = letter - 1 >= 0;
        var nextLetterExists = letter + 1 < goal.Length;

        // Forward
        if (letter == row)
            if (AdjacentAbove(face, row, col, out var neighbor) &&
                nextLetterExists &&
                neighbor.Letter == goal[letter + 1])
                return true;

        // Reverse
        if (letter == goal.Length - 1 - row)
            if (AdjacentAbove(face, row, col, out var neighbor) &&
                prevLetterExists &&
                neighbor.Letter == goal[letter - 1])
                return true;

        // Recursion on substitutes
        if (searchAliases)
            foreach (var dupe in duplicateLetters[letter])
            {
                var complementaryLink = CheckLinkAbove(face, row, col, dupe, false);
                if (complementaryLink) return true;
            }

        return false;
    }

    private bool CheckLinkBelow(CubeSurface face, int row, int col, int letter, bool searchAliases)
    {
        var prevLetterExists = letter - 1 >= 0;
        var nextLetterExists = letter + 1 < goal.Length;

        // Forward
        if (letter == row)
            if (AdjacentBelow(face, row, col, out var neighbor) &&
                prevLetterExists &&
                neighbor.Letter == goal[letter - 1])
                return true;

        // Reverse
        if (letter == goal.Length - 1 - row)
            if (AdjacentBelow(face, row, col, out var neighbor) &&
                nextLetterExists &&
                neighbor.Letter == goal[letter + 1])
                return true;

        // Recursion on substitutes
        if (searchAliases)
            foreach (var dupe in duplicateLetters[letter])
            {
                var complementaryLink = CheckLinkBelow(face, row, col, dupe, false);
                if (complementaryLink) return true;
            }

        return false;
    }

    private bool CheckLinkLeft(CubeSurface face, int row, int col, int letter, bool searchAliases)
    {
        var prevLetterExists = letter - 1 >= 0;
        var nextLetterExists = letter + 1 < goal.Length;

        // Forward
        if (letter == col)
            if (AdjacentLeft(face, row, col, out var neighbor) &&
                prevLetterExists &&
                neighbor.Letter == goal[letter - 1])
                return true;

        // Reverse
        if (letter == goal.Length - 1 - col)
            if (AdjacentLeft(face, row, col, out var neighbor) &&
                nextLetterExists &&
                neighbor.Letter == goal[letter + 1])
                return true;

        // Recursion on substitutes
        if (searchAliases)
            foreach (var dupe in duplicateLetters[letter])
            {
                var complementaryLink = CheckLinkLeft(face, row, col, dupe, false);
                if (complementaryLink) return true;
            }

        return false;
    }

    private bool CheckLinkRight(CubeSurface face, int row, int col, int letter, bool searchAliases)
    {
        var prevLetterExists = letter - 1 >= 0;
        var nextLetterExists = letter + 1 < goal.Length;

        // Forward
        if (letter == col)
            if (AdjacentRight(face, row, col, out var neighbor) &&
                nextLetterExists &&
                neighbor.Letter == goal[letter + 1])
                return true;

        // Reverse
        if (letter == goal.Length - 1 - col)
            if (AdjacentRight(face, row, col, out var neighbor) &&
                prevLetterExists &&
                neighbor.Letter == goal[letter - 1])
                return true;

        // Recursion on substitutes
        if (searchAliases)
            foreach (var dupe in duplicateLetters[letter])
            {
                var complementaryLink = CheckLinkRight(face, row, col, dupe, false);
                if (complementaryLink) return true;
            }

        return false;
    }

    #endregion
}