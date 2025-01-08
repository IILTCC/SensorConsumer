using Confluent.Kafka;
using MongoConsumerLibary.KafkaConsumer;
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
        public SensorConsumer()
        {
            ConfigProvider configProvider = ConfigProvider.Instance;
            _kafkaSettings = configProvider.ProvideKafkaSettings();
            _kafkaConnection = new KafkaConnection(_kafkaSettings);
            _httpConnection = new HttpConnection();
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
    }
    
}
