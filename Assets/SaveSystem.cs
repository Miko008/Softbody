using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveSystem
{
    public static readonly string SAVE_FOLDER = Application.dataPath + "/Saves/";
    static Vector3[][] saveTab;
    static int tabCounter;
    static string saveName;



    public static void Init(int length, string _saveName)
    {
        if(!Directory.Exists(SAVE_FOLDER))
        {
            Directory.CreateDirectory(SAVE_FOLDER);
        }
        saveName = _saveName;
        saveTab = new Vector3[length][];
        tabCounter = 0;
    }

    public static void AddRow(Vector3[] vertexData)
    {
        saveTab[tabCounter] = new Vector3[vertexData.Length];
        for (int i = 0; i < vertexData.Length; i++)
        {
            saveTab[tabCounter][i] = vertexData[i];
        }
        tabCounter++;
    }

    public static void ResetRows()
    {
        tabCounter = 0;
    }
    
    public static void Save()
    {
        int saveNumber = 1;
        while (File.Exists(SAVE_FOLDER + saveName + saveNumber + ".csv"))
        {
            //Debug.Log(saveNumber);
            saveNumber++;
        }
        StreamWriter sw = new StreamWriter(SAVE_FOLDER +  saveName + saveNumber + ".csv");
        for (int i = 0; i < saveTab.Length; i++)
        {
            for (int j = 0; j < saveTab[i].Length; j++)
            {
                sw.Write(saveTab[i][j].ToString());
            }   
            sw.Write(";\n");
        }
        sw.Close();
        Debug.Log("saved as " + saveName + saveNumber);
        tabCounter = 0;
    }
}
