using RabbitMQ.Client;
using System.Text;

namespace ApiService.Services
{
    public class RabbitMqProducer
    {
        private readonly IModel _channel;

        public RabbitMqProducer()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var connection = factory.CreateConnection();
            _channel = connection.CreateModel();
            _channel.ExchangeDeclare(exchange: "task_exchange", type: "fanout");
        }

        public void Publish(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "task_exchange", routingKey: "", basicProperties: null, body: body);
        }
    }
}