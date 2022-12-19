using System;
using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;

namespace ConnectinnoGames.Managers 
{
    public static class SaveManager
    {
        private const string FileType = ".connectinnoSave";
        private static string SavePath => Application.persistentDataPath + "/Saves/";

        public static void SaveData<T>(T data, string fileName)
        {
            Directory.CreateDirectory(SavePath);

            Save(SavePath);

            void Save(string path)
            {
                using var writer = new StreamWriter(path + fileName + FileType);
                var formatter = new BinaryFormatter();
                var memoryStream = new MemoryStream();
                formatter.Serialize(memoryStream, data);
                var dataToSave = Convert.ToBase64String(memoryStream.ToArray());
                writer.WriteLine(dataToSave);
            }
        }

        public static T LoadData<T>(string fileName)
        {
            if (!SaveExists(fileName))
            {
                ConnectinnoGameData gameData = new ConnectinnoGameData();
                SaveData<ConnectinnoGameData>(gameData, "SaveData");
            }
            Directory.CreateDirectory(SavePath);

            T dataToReturn;

            Load(SavePath);

            return dataToReturn;

            void Load(string path)
            {
                using var reader = new StreamReader(path + fileName + FileType);
                var formatter = new BinaryFormatter();
                var dataToLoad = reader.ReadToEnd();
                var memoryStream = new MemoryStream(Convert.FromBase64String(dataToLoad));
                dataToReturn = (T)formatter.Deserialize(memoryStream);
            }
        }

        public static bool SaveExists(string fileName) => File.Exists(SavePath + fileName + FileType);
    }
}