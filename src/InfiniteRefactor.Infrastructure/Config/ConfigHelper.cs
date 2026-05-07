using System.Collections.Specialized;

namespace InfiniteRefactor.Infrastructure.Config
{
    public abstract class ConfigHelper
    {
        public virtual T Value<T>(string key, T defaultValue = default(T))
        {
            return AppSettings(key, defaultValue);
        }
        public abstract NameValueCollection GetNameValueConfiguration(string sectionName);
        public abstract T GetSection<T>(string sectionName) where T : class;

        public abstract string AppSettings(string key, string defaultValue, bool allowNullResult);

        public abstract T AppSettings<T>(string key, T defaultValue = default(T));

        public abstract string ConnectionStrings(string key);

        public static ConfigHelper Instance;

        public static ConfigHelper UseClassicConfig()
        {
            return Instance = new ClassicConfigFileProvider();
        }

        public static ConfigHelper UseJsonConfig()
        {
            return Instance = new JsonConfigFileProvider();
        }

        public static ConfigHelper UseJsonConfig(string productConfig = "appsettings", string developConfig = "appsettings.Development", string basePath = null)
        {
            return Instance = new JsonConfigFileProvider(productConfig, developConfig, basePath);
        }

        static ConfigHelper()
        {
            UseJsonConfig();
            //UseClassicConfig();
        }
    }


}
