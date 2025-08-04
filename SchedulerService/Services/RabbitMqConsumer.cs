using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace SchedulerService.Services
{
    public class RabbitMqConsumer
    {
        public async Task StartAsync()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var connection = await factory.CreateConnectionAsync();

            // --------- 1. Consumer pentru scheduling ---------
            var scheduleChannel = await connection.CreateChannelAsync();
            await scheduleChannel.QueueDeclareAsync("schedule_queue", durable: false, exclusive: false, autoDelete: false);

            var scheduleConsumer = new AsyncEventingBasicConsumer(scheduleChannel);
            scheduleConsumer.ReceivedAsync += async (sender, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var data = JsonSerializer.Deserialize<Dictionary<string, string>>(message);

                if (data != null && data.ContainsKey("task_id") && data.ContainsKey("action"))
                {
                    var taskId = data["task_id"];
                    var action = data["action"];
                    Console.WriteLine($"[Schedule] Received '{action}' for task {taskId}");

                    TaskSchedulerService.UpdateTaskStatus(taskId, action == "schedule");
                }

                await Task.Yield();
            };

            await scheduleChannel.BasicConsumeAsync("schedule_queue", autoAck: true, consumer: scheduleConsumer);

            // --------- 2. Consumer pentru notificări fanout ---------
            var notifyChannel = await connection.CreateChannelAsync();
            await notifyChannel.ExchangeDeclareAsync(exchange: "task_exchange", type: "fanout");
            var queueOk = await notifyChannel.QueueDeclareAsync();
            var notifyQueue = queueOk.QueueName;
            await notifyChannel.QueueBindAsync(queue: notifyQueue, exchange: "task_exchange", routingKey: "");

            var notifyConsumer = new AsyncEventingBasicConsumer(notifyChannel);
            notifyConsumer.ReceivedAsync += async (sender, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"[Notify] Broadcast received: {message}");
                await Task.Yield();
            };

            await notifyChannel.BasicConsumeAsync(queue: notifyQueue, autoAck: true, consumer: notifyConsumer);

            Console.WriteLine(" [*] Waiting for schedule messages and notifications...");
        }
    }
}
