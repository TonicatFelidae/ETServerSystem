using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ETServerSystem.Sample
{
    // A simple device ID provider implementation
    public class SampleDeviceIdProvider : IDeviceIdProvider
    {
        public string GetDeviceId()
        {
            // In a real project, this might retrieve a custom UUID from PlayerPrefs or SDK
            string customId = PlayerPrefs.GetString("Sample_PlayerUUID", string.Empty);
            if (string.IsNullOrEmpty(customId))
            {
                customId = Guid.NewGuid().ToString();
                PlayerPrefs.SetString("Sample_PlayerUUID", customId);
                PlayerPrefs.Save();
            }
            return customId;
        }
    }

    // A simple XOR encryption provider implementation
    public class SampleEncryptionProvider : IEncryptionProvider
    {
        private const string Key = "ETServerSystemKey";

        public string Encrypt(string plainText)
        {
            var result = new StringBuilder();
            for (int i = 0; i < plainText.Length; i++)
            {
                result.Append((char)(plainText[i] ^ Key[i % Key.Length]));
            }
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(result.ToString()));
        }
    }

    // HTTP Implementation of the sample service
    public class SampleHttpServerService : ETHttpServerService, ISampleServerService
    {
        public SampleHttpServerService(string baseUrl, IDeviceIdProvider deviceIdProvider, IEncryptionProvider encryptionProvider)
            : base(baseUrl, deviceIdProvider, encryptionProvider)
        {
        }

        public async Task<SampleData> LoadDataAsync()
        {
            try
            {
                var data = await GetFromServerAsync<SampleData>("player-data", "LoadData");
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SampleHttpServerService] Failed to load data: {ex.Message}");
                SuspendWrites();
                return new SampleData();
            }
        }

        public async Task SaveDataAsync(SampleData data)
        {
            await PutToServerAsync("player-data", data, "SaveData", encrypt: true);
        }

        public async Task<bool> PingServerAsync()
        {
            try
            {
                var response = await GetFromServerAsync<PingResponse>("ping", "PingServer");
                return response != null && response.status == "ok";
            }
            catch
            {
                return false;
            }
        }

        public async Task ResetDataAsync()
        {
            await DeleteFromServerAsync("player-data", "ResetData");
        }

        [Serializable]
        private class PingResponse
        {
            public string status;
        }
    }

    // Mock Offline Implementation of the sample service
    public class SampleMockServerService : ETMockServerService, ISampleServerService
    {
        private const string DATA_FILE = "sample_player_data.json";

        public SampleMockServerService() : base("sample-mock-server")
        {
        }

        public Task<SampleData> LoadDataAsync()
        {
            var data = LoadDomainFile<SampleData>(DATA_FILE);
            return Task.FromResult(data);
        }

        public Task SaveDataAsync(SampleData data)
        {
            SaveDomainFile(DATA_FILE, data);
            return Task.CompletedTask;
        }

        public Task<bool> PingServerAsync()
        {
            return Task.FromResult(true);
        }

        public Task ResetDataAsync()
        {
            DeleteDomainFile(DATA_FILE);
            return Task.CompletedTask;
        }
    }
}

/*
// === VContainer Registration Example ===
// Place this inside your game's DI installer (e.g. ProjectInstaller.cs)

using VContainer;
using VContainer.Unity;
using ETServerSystem;
using ETServerSystem.Sample;

public class SampleInstaller : LifetimeScope
{
    [SerializeField] private SampleApiConfig apiConfig;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(apiConfig).As<IApiConfig>();
        builder.Register<SampleDeviceIdProvider>(Lifetime.Singleton).As<IDeviceIdProvider>();
        builder.Register<SampleEncryptionProvider>(Lifetime.Singleton).As<IEncryptionProvider>();

        if (apiConfig.IsMockEnvironment)
        {
            builder.Register<ISampleServerService, SampleMockServerService>(Lifetime.Scoped);
        }
        else
        {
            builder.Register<ISampleServerService>(container => 
            {
                var deviceIdProvider = container.Resolve<IDeviceIdProvider>();
                var encryptionProvider = container.Resolve<IEncryptionProvider>();
                return new SampleHttpServerService(apiConfig.BaseUrl, deviceIdProvider, encryptionProvider);
            }, Lifetime.Scoped);
        }
    }
}
*/
