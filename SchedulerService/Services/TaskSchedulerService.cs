// // using Grpc.Core;
// // using ProtoEmpty = Google.Protobuf.WellKnownTypes.Empty; // alias pentru Empty
// // using GrpcTaskScheduler = SchedulerService.TaskScheduler; // alias pentru TaskScheduler din proto
// // using RabbitMQ.Client;
// // using System.Collections.Concurrent;
// // using System.Text;
// // using System.Text.Json;

// // namespace SchedulerService.Services
// // {
// //     public class TaskSchedulerService : GrpcTaskScheduler.TaskSchedulerBase
// //     {
// //         private static readonly ConcurrentDictionary<string, TaskResponse> Tasks = new();

// //         private readonly IConnection _connection;
// //         private readonly IModel _channel;

// //         public TaskSchedulerService()
// //         {
// //             var factory = new ConnectionFactory() { HostName = "localhost" };
// //             _connection = factory.CreateConnection();
// //             _channel = _connection.CreateModel();

// //             _channel.QueueDeclare(queue: "schedule_queue", durable: false, exclusive: false, autoDelete: false);
// //             _channel.ExchangeDeclare(exchange: "task_exchange", type: ExchangeType.Fanout);
// //         }

// //         // ---------------- gRPC APIs ---------------- //

// //         public override Task<TaskResponse> CreateTask(TaskRequest request, ServerCallContext context)
// //         {
// //             if (string.IsNullOrWhiteSpace(request.Id) || string.IsNullOrWhiteSpace(request.Name))
// //                 throw new RpcException(new Status(StatusCode.InvalidArgument, "Id și Name sunt obligatorii."));

// //             var task = new TaskResponse
// //             {
// //                 Id = request.Id,
// //                 Name = request.Name,
// //                 IsRunning = false
// //             };

// //             Tasks[request.Id] = task;
// //             Console.WriteLine($"[INFO] Task creat: {task.Id} - {task.Name}");
// //             return Task.FromResult(task);
// //         }

// //         public override Task<ProtoEmpty> DeleteTask(TaskDeleteRequest request, ServerCallContext context)
// //         {
// //             if (Tasks.TryRemove(request.Id, out _))
// //                 Console.WriteLine($"[INFO] Task șters: {request.Id}");
// //             else
// //                 Console.WriteLine($"[WARN] Task {request.Id} nu a fost găsit pentru ștergere");

// //             return Task.FromResult(new ProtoEmpty());
// //         }

// //         public override Task<TaskListResponse> GetAllTasks(ProtoEmpty request, ServerCallContext context)
// //         {
// //             var response = new TaskListResponse();
// //             response.Tasks.AddRange(Tasks.Values);

// //             Console.WriteLine($"[INFO] Returnate {response.Tasks.Count} task-uri.");
// //             return Task.FromResult(response);
// //         }

// //         public override Task<TaskListResponse> GetRunningTasks(ProtoEmpty request, ServerCallContext context)
// //         {
// //             var response = new TaskListResponse();
// //             response.Tasks.AddRange(Tasks.Values.Where(t => t.IsRunning));

// //             Console.WriteLine($"[INFO] Returnate {response.Tasks.Count} task-uri care rulează.");
// //             return Task.FromResult(response);
// //         }

// //         public override Task<ProtoEmpty> ScheduleTask(TaskId request, ServerCallContext context)
// //         {
// //             UpdateTaskStatus(request.Id, true);
// //             PublishTaskEvent(request.Id, "schedule");
// //             return Task.FromResult(new ProtoEmpty());
// //         }

// //         public override Task<ProtoEmpty> UnscheduleTask(TaskId request, ServerCallContext context)
// //         {
// //             UpdateTaskStatus(request.Id, false);
// //             PublishTaskEvent(request.Id, "unschedule");
// //             return Task.FromResult(new ProtoEmpty());
// //         }

// //         // ---------------- Helper methods ---------------- //

// //         public static void UpdateTaskStatus(string taskId, bool isRunning)
// //         {
// //             if (Tasks.TryGetValue(taskId, out var task))
// //             {
// //                 task.IsRunning = isRunning;
// //                 Console.WriteLine($"[TaskSchedulerService] Task {taskId} IsRunning set to {isRunning}");
// //             }
// //             else
// //             {
// //                 Console.WriteLine($"[TaskSchedulerService] Task {taskId} not found for update.");
// //             }
// //         }

// //         private void PublishTaskEvent(string taskId, string action)
// //         {
// //             var message = JsonSerializer.Serialize(new { task_id = taskId, action });
// //             var body = Encoding.UTF8.GetBytes(message);

// //             _channel.BasicPublish(exchange: "", routingKey: "schedule_queue", basicProperties: null, body: body);
// //             _channel.BasicPublish(exchange: "task_exchange", routingKey: "", basicProperties: null, body: body);
// //         }
// //     }
// // }
// using Grpc.Core;
// using ProtoEmpty = Google.Protobuf.WellKnownTypes.Empty;
// using GrpcTaskScheduler = SchedulerService.TaskScheduler;
// using RabbitMQ.Client;
// using System.Collections.Concurrent;
// using System.Text;
// using System.Text.Json;

// namespace SchedulerService.Services
// {
//     public class TaskSchedulerService : GrpcTaskScheduler.TaskSchedulerBase, IDisposable
//     {
//         public static readonly ConcurrentDictionary<string, TaskResponse> Tasks = new();

//         public readonly IConnection _connection;
//         private readonly IModel _channel;

//         public TaskSchedulerService()
//         {
//             try
//             {
//                 var factory = new ConnectionFactory() { HostName = "localhost" };
//                 _connection = factory.CreateConnection();
//                 _channel = _connection.CreateModel();

//                 _channel.QueueDeclare(queue: "schedule_queue", durable: false, exclusive: false, autoDelete: false);
//                 _channel.ExchangeDeclare(exchange: "task_exchange", type: ExchangeType.Fanout);

//                 Console.WriteLine("[INFO] Connected to RabbitMQ.");
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"[ERROR] Could not connect to RabbitMQ: {ex.Message}");
//                 throw;
//             }
//         }

//         public override Task<TaskResponse> CreateTask(TaskRequest request, ServerCallContext context)
//         {
//             if (string.IsNullOrWhiteSpace(request.Id) || string.IsNullOrWhiteSpace(request.Name))
//                 throw new RpcException(new Status(StatusCode.InvalidArgument, "Id și Name sunt obligatorii."));

//             var task = new TaskResponse
//             {
//                 Id = request.Id,
//                 Name = request.Name,
//                 IsRunning = false
//             };

//             Tasks[request.Id] = task;
//             Console.WriteLine($"[INFO] Task creat: {task.Id} - {task.Name}");
//             return Task.FromResult(task);
//         }

//         public override Task<ProtoEmpty> DeleteTask(TaskDeleteRequest request, ServerCallContext context)
//         {
//             if (Tasks.TryRemove(request.Id, out _))
//                 Console.WriteLine($"[INFO] Task șters: {request.Id}");
//             else
//                 Console.WriteLine($"[WARN] Task {request.Id} nu a fost găsit pentru ștergere");

//             return Task.FromResult(new ProtoEmpty());
//         }

//         public override Task<TaskListResponse> GetAllTasks(ProtoEmpty request, ServerCallContext context)
//         {
//             var response = new TaskListResponse();
//             response.Tasks.AddRange(Tasks.Values);

//             Console.WriteLine($"[INFO] Returnate {response.Tasks.Count} task-uri.");
//             return Task.FromResult(response);
//         }

//         public override Task<TaskListResponse> GetRunningTasks(ProtoEmpty request, ServerCallContext context)
//         {
//             var response = new TaskListResponse();
//             response.Tasks.AddRange(Tasks.Values.Where(t => t.IsRunning));

//             Console.WriteLine($"[INFO] Returnate {response.Tasks.Count} task-uri care rulează.");
//             return Task.FromResult(response);
//         }

//         public override Task<ProtoEmpty> ScheduleTask(TaskId request, ServerCallContext context)
//         {
//             UpdateTaskStatus(request.Id, true);
//             SafePublishTaskEvent(request.Id, "schedule");
//             return Task.FromResult(new ProtoEmpty());
//         }

//         public override Task<ProtoEmpty> UnscheduleTask(TaskId request, ServerCallContext context)
//         {
//             UpdateTaskStatus(request.Id, false);
//             SafePublishTaskEvent(request.Id, "unschedule");
//             return Task.FromResult(new ProtoEmpty());
//         }

//         // ---------------- Helper methods ---------------- //

//         public static void UpdateTaskStatus(string taskId, bool isRunning)
//         {
//             if (Tasks.TryGetValue(taskId, out var task))
//             {
//                 task.IsRunning = isRunning;
//                 Console.WriteLine($"[TaskSchedulerService] Task {taskId} IsRunning set to {isRunning}");
//             }
//             else
//             {
//                 Console.WriteLine($"[TaskSchedulerService] Task {taskId} not found for update.");
//             }
//         }

//         private void SafePublishTaskEvent(string taskId, string action)
//         {
//             try
//             {
//                 var message = JsonSerializer.Serialize(new { task_id = taskId, action });
//                 var body = Encoding.UTF8.GetBytes(message);

//                 _channel.BasicPublish(exchange: "", routingKey: "schedule_queue", basicProperties: null, body: body);
//                 _channel.BasicPublish(exchange: "task_exchange", routingKey: "", basicProperties: null, body: body);

//                 Console.WriteLine($"[INFO] Event published: {action} -> {taskId}");
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"[ERROR] Failed to publish RabbitMQ message: {ex.Message}");
//             }
//         }

//         public void Dispose()
//         {
//             _channel?.Close();
//             _connection?.Close();
//             Console.WriteLine("[INFO] RabbitMQ connection closed.");
//         }
//     }
// }
using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using RabbitMQ.Client;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace SchedulerService.Services
{
    public class TaskSchedulerService : TaskScheduler.TaskSchedulerBase, IDisposable
    {
        private static readonly ConcurrentDictionary<string, TaskResponse> Tasks = new();

        private readonly IConnection _connection;
        private readonly IModel _channel;

        public TaskSchedulerService()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = (IConnection?)factory.CreateConnectionAsync();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: "schedule_queue", durable: false, exclusive: false, autoDelete: false);
            _channel.ExchangeDeclare(exchange: "task_exchange", type: ExchangeType.Fanout);

            Console.WriteLine("[INFO] RabbitMQ connected (sync).");
        }

        public override Task<TaskResponse> CreateTask(TaskRequest request, ServerCallContext context)
        {
            if (string.IsNullOrWhiteSpace(request.Id) || string.IsNullOrWhiteSpace(request.Name))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Id și Name sunt obligatorii."));

            var task = new TaskResponse
            {
                Id = request.Id,
                Name = request.Name,
                IsRunning = false
            };

            Tasks[request.Id] = task;
            Console.WriteLine($"[INFO] Task creat: {task.Id} - {task.Name}");
            return Task.FromResult(task);
        }

        public override Task<Empty> DeleteTask(TaskDeleteRequest request, ServerCallContext context)
        {
            if (Tasks.TryRemove(request.Id, out _))
                Console.WriteLine($"[INFO] Task șters: {request.Id}");
            else
                Console.WriteLine($"[WARN] Task {request.Id} nu a fost găsit pentru ștergere");

            return Task.FromResult(new Empty());
        }

        public override Task<TaskListResponse> GetAllTasks(Empty request, ServerCallContext context)
        {
            var response = new TaskListResponse();
            response.Tasks.AddRange(Tasks.Values);
            return Task.FromResult(response);
        }

        public override Task<TaskListResponse> GetRunningTasks(Empty request, ServerCallContext context)
        {
            var response = new TaskListResponse();
            response.Tasks.AddRange(Tasks.Values.Where(t => t.IsRunning));
            return Task.FromResult(response);
        }

        public override Task<Empty> ScheduleTask(TaskId request, ServerCallContext context)
        {
            UpdateTaskStatus(request.Id, true);
            PublishTaskEvent(request.Id, "schedule");
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> UnscheduleTask(TaskId request, ServerCallContext context)
        {
            UpdateTaskStatus(request.Id, false);
            PublishTaskEvent(request.Id, "unschedule");
            return Task.FromResult(new Empty());
        }

        public static void UpdateTaskStatus(string taskId, bool isRunning)
        {
            if (Tasks.TryGetValue(taskId, out var task))
            {
                task.IsRunning = isRunning;
                Console.WriteLine($"[TaskSchedulerService] Task {taskId} IsRunning set to {isRunning}");
            }
        }

        private void PublishTaskEvent(string taskId, string action)
        {
            var message = JsonSerializer.Serialize(new { task_id = taskId, action });
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: "", routingKey: "schedule_queue", basicProperties: null, body: body);
            _channel.BasicPublish(exchange: "task_exchange", routingKey: "", basicProperties: null, body: body);
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.CloseAsync();
            Console.WriteLine("[INFO] RabbitMQ connection closed.");
        }
    }
}
