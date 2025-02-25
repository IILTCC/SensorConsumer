using Confluent.Kafka;
using MongoConsumerLibary.KafkaConsumer;
using MongoConsumerLibary.MongoConnection;
using MongoConsumerLibary.MongoConnection.Collections.PropetyClass;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SensorConsumer.AppSettings;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SensorConsumer
{
    class SensorConsumer
    {
        private readonly KafkaSettings _kafkaSettings;
        private readonly KafkaConnection _kafkaConnection;
        private readonly HttpConnection _httpConnection;
        private readonly MongoConnection _mongoConnection;
        private readonly MongoSettings _mongoSettings;
        public SensorConsumer()
        {
            ConfigProvider configProvider = ConfigProvider.Instance;
            _kafkaSettings = configProvider.ProvideKafkaSettings();
            _kafkaConnection = new KafkaConnection(_kafkaSettings);
            _httpConnection = new HttpConnection();
            _mongoSettings = configProvider.ProvideMongoSettings();
            _mongoConnection = new MongoConnection(_mongoSettings);
            _mongoConnection.WaitForMongoConnection();
        }

        public List<string> InitializeTopicNames()
        {
            List<string> topicNames = new List<string>();
            foreach (string topic in _kafkaSettings.KafkaTopics)
                topicNames.Add(topic);
            return topicNames;
        }
        public async Task StartConsumer()
        {
            _kafkaConnection.WaitForKafkaConnection();
            IConsumer<Ignore, string> consumer = _kafkaConnection.Consumer(InitializeTopicNames());
            CancellationToken cancellationToken = _kafkaConnection.CancellationToken(consumer);
            while (true)
            {
                try
                {
                    ConsumeResult<Ignore, string> consumerResult = consumer.Consume(cancellationToken);

                    StatisticCollection statisticDocument = ConvertToStatisticDocument(consumerResult.Message.Value);
                    _mongoConnection.AddDocument(statisticDocument,_mongoSettings.DocumentTTL);
                    await _httpConnection.SendRequestAsync(consumerResult.Message.Value);
                }
                catch (KafkaException e)
                {
                }
                catch (Exception e)
                {
                }
            }
        }
        private StatisticCollection ConvertToStatisticDocument(string json)
        {
            StatisticCollection temp = new StatisticCollection();
            try
            {
                JObject jsonObject = JObject.Parse(json);
                DateTime timestamp = jsonObject[Consts.STATISTICS_TIMESTAMP_NAME].ToObject<DateTime>();
                jsonObject.Remove(Consts.STATISTICS_TIMESTAMP_NAME);
                JObject wrappedJson = new JObject
                {
                    [nameof(temp.StatisticValues)] = jsonObject,
                };
                StatisticCollection statistic =  JsonConvert.DeserializeObject<StatisticCollection>(wrappedJson.ToString());
                statistic.RealTime = timestamp;
                return statistic;
            }
            catch(Exception e)
            {
                return temp;
            }
        }
    }
    
}
