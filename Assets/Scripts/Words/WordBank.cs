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

using SimpleJSON;
using System.Collections.Generic;
using UnityEngine;

public static class WordBank
{
    private static readonly WordInfo[][] Banks     = new WordInfo[3][];
    private static readonly string[]     FileNames = new [] { "dict-easy", "dict-normal", "dict-hard" };

    public static void LoadIntoMemory()
    {
        Banks[PuzzleDifficulty.Easy.ToInt()] = LoadWords(PuzzleDifficulty.Easy);
        Banks[PuzzleDifficulty.Normal.ToInt()] = LoadWords(PuzzleDifficulty.Normal);
        Banks[PuzzleDifficulty.Hard.ToInt()] = LoadWords(PuzzleDifficulty.Hard);
    }

    private static WordInfo[] GetBank(PuzzleDifficulty difficulty)
    {
        return Banks[difficulty.ToInt()];
    }

    private static WordInfo[] LoadWords(PuzzleDifficulty difficulty)
    {
        var file = Resources.Load<TextAsset>($"Dictionary/{FileNames[difficulty.ToInt()]}");
        var json = JSON.Parse(file.text);
        var list = new List<WordInfo>();
        foreach (var node in json.Children)
        {
            list.Add(new WordInfo(node["word"], node["pos"], node["pron"], node["def"]));
        }
        return list.ToArray();
    }

    public static WordInfo RandomWord(PuzzleDifficulty difficulty)
    {
        return GetBank(difficulty).RandomItem();
        // return hardWords.RandomItem();
        // return difficulty == PuzzleDifficulty.Easy ?
        //                                             normalWords.RandomIten() :
        //                                             easyWords.RandomItem();
    }
}
