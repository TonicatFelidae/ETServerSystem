using System.Threading.Tasks;

namespace ETServerSystem
{
    public interface IETHttpServerService
    {
        Task<T> GetFromServerAsync<T>(string path, string caller = "Unknown") where T : new();
        Task PutToServerAsync<T>(string path, T body, string caller = "Unknown", bool encrypt = false);
        Task<TResponse> PostToServerAsync<TResponse, TRequest>(string path, TRequest body, string caller = "Unknown") where TResponse : class, new();
        Task DeleteFromServerAsync(string path, string caller = "Unknown");
        void SuspendWrites();
        void ResumeWrites();
        bool AreWritesSuspended();
    }
}
