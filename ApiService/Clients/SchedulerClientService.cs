using Grpc.Net.Client;
using SchedulerService; 
public class SchedulerClientService
{
    private readonly SchedulerService.TaskScheduler.TaskSchedulerClient _client;

    public SchedulerClientService()
    {
        var channel = GrpcChannel.ForAddress("http://localhost:5149"); 
        _client = new SchedulerService.TaskScheduler.TaskSchedulerClient(channel);
    }

    public async Task<TaskResponse> CreateTask(string id, string name)
    {
        return await _client.CreateTaskAsync(new TaskRequest { Id = id, Name = name });
    }

    public async Task DeleteTask(string id)
    {
        await _client.DeleteTaskAsync(new TaskDeleteRequest { Id = id });
    }

    public async Task<TaskListResponse> GetAllTasks()
    {
        return await _client.GetAllTasksAsync(new Empty());
    }

    public async Task<TaskListResponse> GetRunningTasks()
    {
        return await _client.GetRunningTasksAsync(new Empty());
    }
}

internal class TaskDeleteRequest
{
    public string Id { get; set; }
}