using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System;

public class EditorMethods : Editor
{
    const string extension = ".cs";
    public static void WriteToEnum<T>(string path, string name, ICollection<T> data)
    {
        MD5 md5Hasher = MD5.Create();

        using (StreamWriter file = File.CreateText(path + name + extension))
        {
            file.WriteLine("public enum " + name + " \n{");

          //  int i = 0;
            foreach (var line in data)
            {
                string lineRep = line.ToString().Replace(" ", string.Empty);
                if (!string.IsNullOrEmpty(lineRep))
                {
                    
                    var hashed = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(lineRep));
                    int ivalue = BitConverter.ToInt32(hashed, 0);

                    int hashCode = ivalue/2200; //lineRep.GetHashCode()/ 2200;
                    //976 128 max/min hashcode
                    //max min int: 2,147,483,647
                    //             1 122 HHH HHH
                    // 1 = first char
                    // 2 = second char

                    if (lineRep.Length > 1)
                    {
                        int firstChar = ((int)lineRep.ToUpper().ToCharArray()[0]) - 65;
                        int secondChar = ((int)lineRep.ToUpper().ToCharArray()[1]) - 65;

                        hashCode += 100000000 * (Mathf.Min(firstChar, 21)); //No more space =(
                        hashCode += 1000000 * secondChar;
                    }

                    file.WriteLine(string.Format("\t{0} = {1},",
                        lineRep,
                        hashCode
                        //i
                        ));
                 //   i++;
                }
            }

            file.WriteLine("\n}");
        }

        AssetDatabase.ImportAsset(path + name + extension);
    }
}
