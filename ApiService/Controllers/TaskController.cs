using Microsoft.AspNetCore.Mvc;
using SchedulerService; 
using RabbitMQ.Client;
using ApiService.Services;

[ApiController]
[Route("tasks")]
public class TasksController : ControllerBase
{
    private readonly SchedulerClientService _schedulerClient;
    private readonly RabbitMqProducer _producer;

    public TasksController(RabbitMqProducer producer)
    {
        _schedulerClient = new SchedulerClientService();
        _producer = producer;
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] TaskRequest request)
    {
        var response = await _schedulerClient.CreateTask(request.Id, request.Name);
        _producer.Publish($"Task created: {request.Id}");
        return Ok(response);
    }
}
