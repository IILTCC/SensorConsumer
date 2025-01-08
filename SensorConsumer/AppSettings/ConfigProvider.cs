using Microsoft.Extensions.Configuration;
using MongoConsumerLibary;
using MongoConsumerLibary.KafkaConsumer;
using MongoConsumerLibary.MongoConnection;
using System.IO;

namespace SensorConsumer.AppSettings
{
    class ConfigProvider
    {
        private static ConfigProvider _instance;
        private static IConfigurationRoot _configFile;
        private KafkaSettings _kafkaSettings;
        public static ConfigProvider Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ConfigProvider();
                return _instance;
            }
        }
        public ConfigProvider()
        {
            _configFile = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(Consts.APPSETTINGS_PATH, optional: false, reloadOnChange: true)
            .Build();
            _kafkaSettings = _configFile.GetRequiredSection(nameof(KafkaSettings)).Get<KafkaSettings>();
        }
        public KafkaSettings ProvideKafkaSettings()
        {
            return _kafkaSettings;                 
        }

    }
}
