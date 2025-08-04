// // using RabbitMQ.Client;
// // using System.Text;

// // namespace ApiService.Services
// // {
// //     public class RabbitMqProducer
// //     {
// //         private readonly IModel _channel;

// //         public RabbitMqProducer()
// //         {
// //             var factory = new ConnectionFactory() { HostName = "localhost" };
// //             var connection = factory.CreateConnection();
// //             _channel = connection.CreateModel();
// //             _channel.ExchangeDeclare(exchange: "task_exchange", type: "fanout");
// //         }

// //         public void Publish(string message)
// //         {
// //             var body = Encoding.UTF8.GetBytes(message);
// //             _channel.BasicPublish(exchange: "task_exchange", routingKey: "", basicProperties: null, body: body);
// //         }
// //     }

// //     internal interface IModel
// //     {
// //         void BasicPublish(string exchange, string routingKey, object basicProperties, byte[] body);
// //         void ExchangeDeclare(string exchange, string type);
// //     }
// // }
// using RabbitMQ.Client;
// using System.Text;

// namespace ApiService.Services
// {
//     public class RabbitMqProducer : IDisposable
//     {
//         private readonly IConnection _connection;
//         private readonly IModel _channel;

//         public RabbitMqProducer()
//         {
//             var factory = new ConnectionFactory() { HostName = "localhost" };
//             _connection = factory.CreateConnection();
//             _channel = _connection.CreateModel();
//             _channel.ExchangeDeclare(exchange: "task_exchange", type: ExchangeType.Fanout);
//         }

//         public void Publish(string message)
//         {
//             var body = Encoding.UTF8.GetBytes(message);
//             _channel.BasicPublish(
//                 exchange: "task_exchange",
//                 routingKey: "",
//                 basicProperties: null,
//                 body: body
//             );

//             Console.WriteLine($"[Producer] Sent: {message}");
//         }

//         public void Dispose()
//         {
//             _channel?.Close();
//             _connection?.CloseAsync();
//         }
//     }

//     internal interface IModel
//     {
//         void ExchangeDeclare(string exchange, string type);
//     }
// }
using RabbitMQ.Client;
using System.Text;

namespace ApiService.Services
{
    public class RabbitMqProducer : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMqProducer()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(exchange: "task_exchange", type: ExchangeType.Fanout);
        }

        public void Publish(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(
                exchange: "task_exchange",
                routingKey: "",
                basicProperties: null,
                body: body
            );

            Console.WriteLine($"[Producer] Sent: {message}");
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}
