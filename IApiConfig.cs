namespace ETServerSystem
{
    public interface IApiConfig
    {
        ApiEnvironment Environment { get; }
        string BaseUrl { get; }
        bool IsMockEnvironment { get; }
        bool IsLocalEnvironment { get; }
        bool IsStagingEnvironment { get; }
        bool IsProductionEnvironment { get; }
    }
}
