using UnityEngine;

namespace ETServerSystem.Sample
{
    [CreateAssetMenu(fileName = "SampleApiConfig", menuName = "ETServerSystem/Sample API Config")]
    public class SampleApiConfig : ScriptableObject, IApiConfig
    {
        public ApiEnvironment environment = ApiEnvironment.Mock;
        public string localUrl = "http://localhost:3000";
        public string stagingUrl = "https://staging.myserver.com";
        public string productionUrl = "https://myserver.com";

        public ApiEnvironment Environment => environment;

        public string BaseUrl
        {
            get
            {
                switch (environment)
                {
                    case ApiEnvironment.Staging:
                        return stagingUrl;
                    case ApiEnvironment.Production:
                        return productionUrl;
                    default:
                        return localUrl;
                }
            }
        }

        public bool IsMockEnvironment => environment == ApiEnvironment.Mock;
        public bool IsLocalEnvironment => environment == ApiEnvironment.Local;
        public bool IsStagingEnvironment => environment == ApiEnvironment.Staging;
        public bool IsProductionEnvironment => environment == ApiEnvironment.Production;
    }
}
