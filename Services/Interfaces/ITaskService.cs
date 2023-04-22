using TaskService.Domain;
using TaskService.Dto.Task;
using TaskService.Dto.Users.Task;
using Task = System.Threading.Tasks.Task;
using DomainTask = TaskService.Domain.Task;
namespace TaskService.Services.Interfaces;

public interface ITaskService
{
    public Task<TaskShortInfo> CreateTask(CreateTaskRequest request, string userId);
    public Task<TaskInfoResponse> GetTaskInfo(int taskId, string userId);
    public Task<TaskInfoResponse> UpdateTask(int taskId, UpdateTaskRequest request, string userId);
    public Task DeleteTask(int taskId, string userId);
    public Task<(DomainTask task, User user)> AssignUser(UserTaskInterractionRequest request, string userId);
    public Task UnassignUser(UserTaskInterractionRequest request, string userId);
    public Task MarkTaskAsDone(int taskId, string userId);
}