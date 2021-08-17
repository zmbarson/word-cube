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
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

public class PostGame : MonoBehaviour
{
    [SerializeField] private AudioSource    chimeSoundFx;
    [SerializeField] private GameOverScreen gameOverScreen;
    [SerializeField] private CubeExplosion  explosion;

    public bool IsFinished { get; private set; }

    public YieldInstruction OutroSequence(Cube cube, CubeController controller, float time, int moves, int bonus, ReadOnlyCollection<PuzzleSolution> solutions)
    {
        return StartCoroutine(DoOutro(cube, controller, time, moves, bonus, solutions));
    }

    private IEnumerator DoOutro(Cube cube, CubeController controller, float time, int moves, int bonus, ReadOnlyCollection<PuzzleSolution> solutions)
    {
        // Reset the color of all highlighted cubits that are not part of the solution.
        DisableExtraneousLinks(solutions, cube);
        yield return StartCoroutine(AnimateSolutions(controller, solutions));
        yield return explosion.Animate(cube);
        yield return gameOverScreen.Show(cube, time, moves, bonus);
        IsFinished = true;
    }
    
    private IEnumerable<PuzzleSolution> SortByNearest(ReadOnlyCollection<PuzzleSolution> sequences)
    {
        if (sequences.Count > 1)
            return sequences.OrderByDescending(s => Vector3.Dot(s.surface.Normal, Vector3.back));
        return sequences;
    }

    private void DisableExtraneousLinks(ReadOnlyCollection<PuzzleSolution> solutions, Cube cube)
    {
        var faces = solutions.SelectMany(s => s.members).ToList();
        foreach (var face in cube.Faces)
            if (!face.IsBlank && face.IsLinked && !faces.Contains(face))
                face.IsLinked = false;
    }

    private YieldInstruction AnimateLetters(PuzzleSolution solution, int letterNumber, int totalLetters)
    {
        var sequence = DOTween.Sequence();
        for (var j = 0; j < solution.members.Count; j++)
        {
            var pitch  = (float) (letterNumber + j) / (totalLetters - 1);
            var letter = solution[j];
            sequence
               .Append(solution[j]
                          .Parent
                          .Transform
                          .DOPunchScale(Vector3.one * 0.33f, 0.33f, 1, 0f)
                          .OnStart(() =>
                                   {
                                       chimeSoundFx.pitch = Mathf.Lerp(1.5f, 3f, pitch);
                                       chimeSoundFx.Play();
                                       letter.BackgroundColor = App.ColorScheme;
                                   }));
        }
        return sequence.Play().WaitForCompletion();
    }

    private IEnumerator AnimateSolutions(CubeController controller, ReadOnlyCollection<PuzzleSolution> solutions)
    {
        //App.AppMusic.FadeInUIBackground(1f);
        // Reset the color of all highlighted cubits that are not part of the solution.
        var   totalFaces      = solutions.Count * (solutions.First().members.Count);
        var   lettersAnimated = 0;
        var   sorted          = SortByNearest(solutions);
        Axis? lastFace        = null;
        foreach (var solution in sorted)
        {
            if (!lastFace.HasValue || lastFace.Value != solution.surface.axis)
            {
                lastFace = solution.surface.axis;
                yield return controller.FaceCamera(solution.surface.Normal, 0.75f, false);
            }

            // Animate the solution.
            yield return AnimateLetters(solution, lettersAnimated, totalFaces);
            lettersAnimated += solutions.First().members.Count;
        }
    }
}
