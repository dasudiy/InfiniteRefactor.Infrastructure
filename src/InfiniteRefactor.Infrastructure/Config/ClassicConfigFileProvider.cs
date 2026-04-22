using AirMaster.Infrastructure.Extensions;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;

namespace AirMaster.Infrastructure.Config
{
    public class ClassicConfigFileProvider : ConfigHelper
    {
        private Configuration config;

        public static ClassicConfigFileProvider OpenAppConfig()
        {
            return new ClassicConfigFileProvider { config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None) };
        }

        public static ClassicConfigFileProvider OpenAppConfig(string exePath)
        {
            return new ClassicConfigFileProvider { config = ConfigurationManager.OpenExeConfiguration(exePath) };
        }

        public override string AppSettings(string key, string defaultValue, bool allowNullResult)
        {
            if (config == null)
            {
                var ret = ConfigurationManager.AppSettings[key];
                if (ret == null && !allowNullResult)
                {
                    return defaultValue;
                }
                else
                {
                    return ret;
                }
            }
            var c = config.AppSettings.Settings[key];
            if (c != null)
            {
                return c.Value;
            }
            else if (allowNullResult)
            {
                return null;
            }
            else
            {
                return defaultValue;
            }
        }

        public override T AppSettings<T>(string key, T defaultValue = default(T))
        {
            return AppSettings(key, null, true).To<T>(defaultValue);
        }

        public override string ConnectionStrings(string key)
        {
            if (config == null)
            {
                return ConfigurationManager.ConnectionStrings[key]?.ConnectionString;
            }

            var conn = config.ConnectionStrings.ConnectionStrings[key];
            return conn?.ConnectionString;
        }

        public override T GetSection<T>(string sectionName)
        {
            if (config == null)
            {
                return ConfigurationManager.GetSection(sectionName) as T;
            }

            return config.GetSection(sectionName) as T;
        }

        public void SetAppSettings(string key, string value)
        {
            if (config == null)
            {
                throw new Exception("请先打开配置文件");
            }

            if (config.AppSettings.Settings.AllKeys.Contains(key))
            {
                config.AppSettings.Settings[key].Value = value;
            }
            else
            {
                config.AppSettings.Settings.Add(new KeyValueConfigurationElement(key, value));
            }

            SaveChanges();
        }

        public void SaveChanges(string sectionName = "appSettings")
        {
            if (config == null)
            {
                throw new Exception("请先打开配置文件");
            }

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(sectionName);
        }

        public ConnectionStringSettings ConnectionStringSettings(string key)
        {
            if (config == null)
            {
                return ConfigurationManager.ConnectionStrings[key];
            }

            return config.ConnectionStrings.ConnectionStrings[key];
        }

//#if !NETSTANDARD
//        public static ClassicConfigFileProvider OpenWebConfig(string path = null)
//        {
//            path = path ?? System.Web.HttpContext.Current.Response.ApplyAppPathModifier(".");
//            var config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration(path);
//            return new ClassicConfigFileProvider { config = config };
//        }

//        public DbConnection CreateConnectionFromConnStrings(string key)
//        {
//            var connconfig = ConnectionStringSettings(key);
//            if (connconfig != null)
//            {
//                var factory = DbProviderFactories.GetFactory(connconfig.ProviderName);
//                var conn = factory.CreateConnection();
//                conn.ConnectionString = connconfig.ConnectionString;
//                return conn;
//            }
//            return null;
//        }
//#endif

        public override NameValueCollection GetNameValueConfiguration(string sectionName)
        {
            return GetSection<NameValueCollection>(sectionName);
        }

        public T GetNameValueCollectionSectionValue<T>(string sectionName, string keyName, T defaultValue = default(T))
        {
            var section = GetNameValueConfiguration(sectionName);
            if (section == null) { return defaultValue; }
            return section[keyName].To<T>(defaultValue);
        }
    }

    public abstract class AbstractSerializerSectionHandler : System.Configuration.IConfigurationSectionHandler
    {

        #region IConfigurationSectionHandler 成员

        public abstract object Create(object parent, object configContext, System.Xml.XmlNode section);

        #endregion

        protected string GetAttributeValue(string name, System.Xml.XmlNode node)
        {
            return node.Attributes[name] == null ? string.Empty : node.Attributes[name].Value;
        }
    }
    public class SerializerSectionHandler<T> : AbstractSerializerSectionHandler
    {
        public override object Create(object parent, object configContext, System.Xml.XmlNode section)
        {
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
            return serializer.Deserialize(new System.Xml.XmlNodeReader(section));
        }
    }
}
