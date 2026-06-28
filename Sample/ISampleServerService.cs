using System.Threading.Tasks;

namespace ETServerSystem.Sample
{
    public interface ISampleServerService
    {
        Task<SampleData> LoadDataAsync();
        Task SaveDataAsync(SampleData data);
        Task<bool> PingServerAsync();
        Task ResetDataAsync();
    }
}
