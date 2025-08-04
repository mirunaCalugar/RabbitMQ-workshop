using Microsoft.AspNetCore.Mvc;
using Grpc.Net.Client;
using SchedulerService; 
using ProtoEmpty = Google.Protobuf.WellKnownTypes.Empty; 
using GrpcTaskScheduler = SchedulerService.TaskScheduler;

namespace ApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly GrpcTaskScheduler.TaskSchedulerClient _grpcClient;

        public TaskController(IConfiguration configuration)
        {
            var schedulerUrl = configuration.GetValue<string>("SchedulerService:Url") ?? "http://localhost:5001";
            var channel = GrpcChannel.ForAddress(schedulerUrl);
            _grpcClient = new GrpcTaskScheduler.TaskSchedulerClient(channel);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TaskRequest request)
        {
            var response = await _grpcClient.CreateTaskAsync(request);
            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _grpcClient.DeleteTaskAsync(new TaskDeleteRequest { Id = id });
            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var response = await _grpcClient.GetAllTasksAsync(new ProtoEmpty());
            return Ok(response.Tasks);
        }

        [HttpGet("running")]
        public async Task<IActionResult> GetRunning()
        {
            var response = await _grpcClient.GetRunningTasksAsync(new ProtoEmpty());
            return Ok(response.Tasks);
        }

        [HttpPost("{id}/schedule")]
        public async Task<IActionResult> Schedule(string id)
        {
            await _grpcClient.ScheduleTaskAsync(new TaskId { Id = id });
            return Ok(new { message = $"Task {id} scheduled." });
        }

        [HttpPost("{id}/unschedule")]
        public async Task<IActionResult> Unschedule(string id)
        {
            await _grpcClient.UnscheduleTaskAsync(new TaskId { Id = id });
            return Ok(new { message = $"Task {id} unscheduled." });
        }
    }
}
