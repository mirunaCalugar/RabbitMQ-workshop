using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Tasks;

namespace SchedulerService.Services
{
    public class RabbitMqConsumer
    {
        public async Task StartAsync()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(exchange: "task_exchange", type: "fanout");
            var queueOk = await channel.QueueDeclareAsync();
            var queueName = queueOk.QueueName;

            await channel.QueueBindAsync(queue: queueName, exchange: "task_exchange", routingKey: "");

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (sender, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"[x] Received {message}");
                await Task.Yield();
            };

            await channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);

            Console.WriteLine(" [*] Waiting for messages...");
        }
    }
}
