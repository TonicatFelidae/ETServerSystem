using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace ETServerSystem
{
    public class ETMockServerService : IETMockServerService
    {
        protected readonly string mockDirPath;
        protected readonly JsonSerializerSettings jsonSettings;

        public ETMockServerService(string subFolder = "ETServerSystemMock")
        {
            mockDirPath = Path.Combine(Application.persistentDataPath, subFolder);
            EnsureDirectoryExists();

            jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Include
            };
        }

        public ETMockServerService(string customMockDirPath, string defaultSubFolder)
        {
            mockDirPath = !string.IsNullOrEmpty(customMockDirPath)
                ? customMockDirPath
                : Path.Combine(Application.persistentDataPath, defaultSubFolder);

            EnsureDirectoryExists();

            jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Include
            };
        }

        private void EnsureDirectoryExists()
        {
            try
            {
                if (!Directory.Exists(mockDirPath))
                {
                    Directory.CreateDirectory(mockDirPath);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ETMockServerService] Failed to create directory {mockDirPath}: {ex.Message}");
            }
        }

        public T LoadDomainFile<T>(string filename) where T : new()
        {
            var path = Path.Combine(mockDirPath, filename);
            if (!File.Exists(path))
            {
                return new T();
            }

            try
            {
                var json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<T>(json, jsonSettings);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ETMockServerService] Failed to load {filename}: {ex.Message}");
                return new T();
            }
        }

        public void SaveDomainFile<T>(string filename, T data)
        {
            try
            {
                var path = Path.Combine(mockDirPath, filename);
                var json = JsonConvert.SerializeObject(data, jsonSettings);
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ETMockServerService] Failed to save {filename}: {ex.Message}");
            }
        }

        public void DeleteDomainFile(string filename)
        {
            try
            {
                var path = Path.Combine(mockDirPath, filename);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ETMockServerService] Failed to delete {filename}: {ex.Message}");
            }
        }
    }
}
