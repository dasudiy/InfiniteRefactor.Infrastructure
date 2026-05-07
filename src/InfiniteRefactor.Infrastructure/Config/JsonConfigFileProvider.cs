using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using InfiniteRefactor.Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;

namespace InfiniteRefactor.Infrastructure.Config
{
    public class JsonConfigFileProvider : ConfigHelper
    {
        private IConfiguration config;

        public JsonConfigFileProvider(string productConfig = "appsettings", string developConfig = "appsettings.Development", string basePath = null)
        {
            try
            {
                basePath = basePath ?? AppContext.BaseDirectory;

                var builder = new ConfigurationBuilder()
                    .SetBasePath(basePath)
                    //.SetBasePath(Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName))
#if DEBUG
                    .AddJsonFile(productConfig + ".json", optional: true, reloadOnChange: true)
                    .AddJsonFile(developConfig + ".json", optional: true, reloadOnChange: true);

#else
                    .AddJsonFile(productConfig + ".json", optional: true, reloadOnChange: true);
#endif
                builder = builder.AddEnvironmentVariables();
                builder = builder.AddCommandLine(Environment.GetCommandLineArgs());
                config = builder.Build();

            }
            catch (FileNotFoundException)
            {
            }
            catch (FormatException)
            {
            }
        }

        public JsonConfigFileProvider(IConfiguration configuration)
        {
            this.config = configuration;
        }

        public override T Value<T>(string key, T defaultValue)
        {
            var v = config[$"{key}"];
            if (v == null)
            {
                return defaultValue;
            }
            else
            {
                return v.To<T>(defaultValue);
            }
        }

        public override string AppSettings(string key, string defaultValue, bool allowNullResult)
        {
            if (config == null)
            {
                if (allowNullResult)
                {
                    return null;
                }
                else
                {
                    return defaultValue;
                }
            }
            return Value<string>(key, defaultValue);
        }

        public override T AppSettings<T>(string key, T defaultValue = default(T))
        {
            //兼容放在appsettings节下配置                           
            return (AppSettings(key, null, true) ?? AppSettings($"AppSettings:{key}", null, true)).To<T>(defaultValue);
        }

        public override string ConnectionStrings(string key)
        {
            if (config == null) { return null; }
            return config[$"ConnectionStrings:{key}"] ?? config[$"ConnectionStrings:{key}:ConnectionString"];
        }

        public override T GetSection<T>(string sectionName)
        {
            if (config == null) { return null; }
            return config.GetSection(sectionName).Get<T>();
        }

        public IConfiguration GetSection(string sectionName)
        {
            if (config == null) { return null; }
            return config.GetSection(sectionName);
        }

        public override NameValueCollection GetNameValueConfiguration(string sectionName)
        {
            var ret = new NameValueCollection();
            foreach (var item in GetSection<Dictionary<string, string>>(sectionName))
            {
                ret[item.Key] = item.Value;
            }
            return ret;
        }
    }
}
