using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using System.Collections.Concurrent;
using System.Linq;

namespace SchedulerService.Services
{
    public class TaskSchedulerService : TaskScheduler.TaskSchedulerBase
    {
        // Stocare thread-safe a task-urilor
        private static readonly ConcurrentDictionary<string, TaskResponse> Tasks = new();

        /// <summary>
        /// Creează un task nou.
        /// </summary>
        public override Task<TaskResponse> CreateTask(TaskRequest request, ServerCallContext context)
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] CreateTask a eșuat: {ex}");
                throw new RpcException(new Status(StatusCode.Internal, "Eroare la crearea task-ului."));
            }
        }

        /// <summary>
        /// Șterge un task după Id.
        /// </summary>
        public override Task<Empty> DeleteTask(TaskDeleteRequest request, ServerCallContext context)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Id))
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Id-ul este obligatoriu."));

                if (Tasks.TryRemove(request.Id, out _))
                    Console.WriteLine($"[INFO] Task șters: {request.Id}");
                else
                    Console.WriteLine($"[WARN] Task inexistent pentru ștergere: {request.Id}");

                return Task.FromResult(new Empty());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] DeleteTask a eșuat: {ex}");
                throw new RpcException(new Status(StatusCode.Internal, "Eroare la ștergerea task-ului."));
            }
        }

        /// <summary>
        /// Returnează toate task-urile existente.
        /// </summary>
        public override Task<TaskListResponse> GetAllTasks(Empty request, ServerCallContext context)
        {
            try
            {
                var response = new TaskListResponse();
                response.Tasks.AddRange(Tasks.Values);

                Console.WriteLine($"[INFO] Returnate {response.Tasks.Count} task-uri.");

                return Task.FromResult(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetAllTasks a eșuat: {ex}");
                throw new RpcException(new Status(StatusCode.Internal, "Eroare la obținerea task-urilor."));
            }
        }

        /// <summary>
        /// Returnează doar task-urile care rulează.
        /// </summary>
        public override Task<TaskListResponse> GetRunningTasks(Empty request, ServerCallContext context)
        {
            try
            {
                var response = new TaskListResponse();
                response.Tasks.AddRange(Tasks.Values.Where(t => t.IsRunning));

                Console.WriteLine($"[INFO] Returnate {response.Tasks.Count} task-uri care rulează.");

                return Task.FromResult(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetRunningTasks a eșuat: {ex}");
                throw new RpcException(new Status(StatusCode.Internal, "Eroare la obținerea task-urilor care rulează."));
            }
        }
    }
}
