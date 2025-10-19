using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    private static string path = Application.persistentDataPath;

    public static void SaveJsonData(string _jsonData, string _fileName)
    {
        string _path = Path.Combine(path, _fileName);

        File.WriteAllText(_path, _jsonData);
    }

    public static string LoadJsonData(string _fileName)
    {
        string _path = Path.Combine(path, _fileName);

        if (!File.Exists(_path))
        {
            InternalDebug.LogError("File not found at: " + _path);
            return null;
        }

        return File.ReadAllText(_path);
    }

    public static void ToBinary<T>(T _data, string _roomName)
    {
        BinaryFormatter formatter = new BinaryFormatter();

        string _path = Path.Combine(path, _roomName);

        using FileStream stream = new FileStream(_path, FileMode.Create);
        formatter.Serialize(stream, _data);
        InternalDebug.Log("Save");
    }

    public static T FromBinary<T>(string _roomName)
    {
        string _path = Path.Combine(path, _roomName);

        if (File.Exists(_path))
        {
            BinaryFormatter formatter = new BinaryFormatter();

            using FileStream stream = new FileStream(_path, FileMode.Open);
            T _data = (T)formatter.Deserialize(stream);
            return _data;
        }
        else
        {
            InternalDebug.LogError("File not found in: " + _path);
            return default;
        }        
    }
}

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        if (wrapper == null)
        {
            return null;
        }
        return wrapper.Items;
    }

    public static string ToJson<T>(T[] array, bool prettyPrint = false)
    {
        return JsonUtility.ToJson(new Wrapper<T> { Items = array }, prettyPrint);
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}
