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

//using System.IO;
//using System.Linq;
//using UnityEditor;
//using UnityEngine;

//public static class CubeSerialization 
//{
//    public static void Serialize(Cube cube, string fileName = "/Resources/savedcube.txt")
//    {
//        var letters = cube.Faces
//                          .Select(f => f.Letter)
//                          .ToArray();
//        var data = new string(letters);
//        File.WriteAllText(Application.dataPath + fileName, cube.Size.ToString() + data);
//        AssetDatabase.SaveAssets();
//        AssetDatabase.Refresh();
//    }

//    public static Cube Deserialize(string fileName = "savedcube")
//    {
//        var file = Resources.Load<TextAsset>(fileName);
//        var data =  file.text
//                               //.Trim()
//                               .ToCharArray();
//        var size = int.Parse(data[0].ToString());
//        var cube = CubeBuilder.Build(size);

//        Debug.Assert(data.Length - 1 == cube.Faces.Count());

//        int i = 1;
//        foreach(var face in cube.Faces)
//        {
//            face.Letter = data[i++];
//        }
//        return cube;
//    }
//}
