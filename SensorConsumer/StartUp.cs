using System.Threading.Tasks;

namespace SensorConsumer
{
    class StartUp
    {
        static async Task Main(string[] args)
        {
            SensorConsumer sensorConsumer = new SensorConsumer();
            await sensorConsumer.StartConsumer();
        }
    }
}
