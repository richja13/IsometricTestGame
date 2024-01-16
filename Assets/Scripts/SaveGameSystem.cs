using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SaveGameSystem
{
    public static void SavePlayerData(List<SaveData> data, string filePath)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + filePath + SceneManager.GetActiveScene().buildIndex;
        string countPath = Application.persistentDataPath + filePath + SceneManager.GetActiveScene().buildIndex;
        FileStream countStream = new FileStream(countPath, FileMode.Create);
        formatter.Serialize(countStream, data.Count);
        countStream.Close();

        for (int i = 0; i < data.Count; i++)
        {
            FileStream stream = new FileStream(path + i, FileMode.Create);
            SaveData itemsData = data[i];
            formatter.Serialize(stream, itemsData);
            stream.Close();
        }

#if UNITY_EDITOR
        Debug.Log("File Saved Successfully");
#endif

    }

    public static List<SaveData> LoadPlayerData(string filePath)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + filePath + SceneManager.GetActiveScene().buildIndex;
        string countPath = Application.persistentDataPath + filePath + SceneManager.GetActiveScene().buildIndex;
        int dataCount = 0;
        List<SaveData> data = new();

        if (File.Exists(countPath))
        {
            FileStream countStream = new FileStream(path, FileMode.Open);
            dataCount = (int)formatter.Deserialize(countStream);
            countStream.Close();

            for (int i = 0; i < dataCount; i++)
            {
                FileStream stream = new FileStream(path + i, FileMode.Open);
                SaveData saveData = formatter.Deserialize(stream) as SaveData;
                data.Add(saveData);
                Debug.Log(saveData.Speed);
                stream.Close();
            }

            return data;
        }
        else
        {
            Debug.LogError("Path not found in" + countPath);
            return null;
        }
    }
}

[Serializable]
public class SaveData
{
    public float Speed;
    public float Agility;
    public float Durability;
}