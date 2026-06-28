namespace ETServerSystem
{
    public interface IETMockServerService
    {
        T LoadDomainFile<T>(string filename) where T : new();
        void SaveDomainFile<T>(string filename, T data);
        void DeleteDomainFile(string filename);
    }
}
