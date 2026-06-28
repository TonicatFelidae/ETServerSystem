using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace ETServerSystem
{
    public class ETHttpServerService : IETHttpServerService
    {
        protected readonly string baseUrl;
        protected readonly JsonSerializerSettings jsonSettings;
        protected readonly IDeviceIdProvider deviceIdProvider;
        protected readonly IEncryptionProvider encryptionProvider;
        
        private static readonly SemaphoreSlim _writeLock = new SemaphoreSlim(1, 1);
        protected bool _writesSuspended;

        public ETHttpServerService(string baseUrl, IDeviceIdProvider deviceIdProvider, IEncryptionProvider encryptionProvider = null)
        {
            this.baseUrl = baseUrl.TrimEnd('/');
            this.deviceIdProvider = deviceIdProvider;
            this.encryptionProvider = encryptionProvider;

            jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Include
            };
        }

        protected string DeviceId => deviceIdProvider != null ? deviceIdProvider.GetDeviceId() : SystemInfo.deviceUniqueIdentifier;

        public void SuspendWrites()
        {
            _writesSuspended = true;
        }

        public void ResumeWrites()
        {
            _writesSuspended = false;
        }

        public bool AreWritesSuspended()
        {
            return _writesSuspended;
        }

        public async Task<T> GetFromServerAsync<T>(string path, string caller = "Unknown") where T : new()
        {
            var url = $"{baseUrl}/{path}";
            using var request = UnityWebRequest.Get(url);
            request.SetRequestHeader("x-device-id", DeviceId);

            var operation = request.SendWebRequest();
            while (!operation.isDone)
                await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[ETHttpServerService] GET {path} failed (caller: {caller}): {request.error}");
                return new T();
            }

            var responseText = request.downloadHandler.text;
            
            OnGetResponseReceived(path, responseText);

            var result = JsonConvert.DeserializeObject<T>(responseText, jsonSettings);
            return result ?? new T();
        }

        protected virtual void OnGetResponseReceived(string path, string responseText)
        {
        }

        public async Task PutToServerAsync<T>(string path, T body, string caller = "Unknown", bool encrypt = false)
        {
            if (_writesSuspended)
            {
                Debug.LogWarning($"[ETHttpServerService] PUT {path} suspended — initial load failed this session; not pushing a possibly-empty state that would wipe server data (caller: {caller}).");
                return;
            }
            await _writeLock.WaitAsync();
            try
            {
                var url = $"{baseUrl}/{path}";
                var json = JsonConvert.SerializeObject(body, jsonSettings);

                string payload;
                if (encrypt && encryptionProvider != null)
                {
                    var encrypted = encryptionProvider.Encrypt(json);
                    var wrapper = new { data = encrypted };
                    payload = JsonConvert.SerializeObject(wrapper);
                }
                else
                {
                    payload = json;
                }
                var bodyBytes = Encoding.UTF8.GetBytes(payload);

                using var request = new UnityWebRequest(url, "PUT");
                request.uploadHandler = new UploadHandlerRaw(bodyBytes);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("x-device-id", DeviceId);

                var operation = request.SendWebRequest();
                while (!operation.isDone)
                    await Task.Yield();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    var responseBody = request.downloadHandler?.text ?? "(no body)";
                    Debug.LogError($"[ETHttpServerService] PUT {path} failed (caller: {caller}): {request.error}\nHTTP {request.responseCode} Response: {responseBody}");
                    return;
                }

                Debug.Log($"[ETHttpServerService] PUT {path} (caller: {caller}) - {json.Length} bytes");
            }
            finally
            {
                _writeLock.Release();
            }
        }

        public async Task<TResponse> PostToServerAsync<TResponse, TRequest>(string path, TRequest body, string caller = "Unknown") where TResponse : class, new()
        {
            await _writeLock.WaitAsync();
            try
            {
                var url = $"{baseUrl}/{path}";
                var json = body != null ? JsonConvert.SerializeObject(body, jsonSettings) : "{}";
                var bodyBytes = Encoding.UTF8.GetBytes(json);

                using var request = new UnityWebRequest(url, "POST");
                request.uploadHandler = new UploadHandlerRaw(bodyBytes);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("x-device-id", DeviceId);

                var operation = request.SendWebRequest();
                while (!operation.isDone)
                    await Task.Yield();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[ETHttpServerService] POST {path} failed (caller: {caller}): {request.error}");
                    return null;
                }

                var responseText = request.downloadHandler.text;
                
                OnPostResponseReceived(path, responseText);

                var result = JsonConvert.DeserializeObject<TResponse>(responseText, jsonSettings);
                return result ?? new TResponse();
            }
            finally
            {
                _writeLock.Release();
            }
        }

        protected virtual void OnPostResponseReceived(string path, string responseText)
        {
        }

        public async Task DeleteFromServerAsync(string path, string caller = "Unknown")
        {
            var url = $"{baseUrl}/{path}";
            using var request = UnityWebRequest.Delete(url);
            request.SetRequestHeader("x-device-id", DeviceId);

            var operation = request.SendWebRequest();
            while (!operation.isDone)
                await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[ETHttpServerService] DELETE {path} failed (caller: {caller}): {request.error}");
                return;
            }

            Debug.Log($"[ETHttpServerService] DELETE {path} (caller: {caller}) succeeded");
        }
    }
}
