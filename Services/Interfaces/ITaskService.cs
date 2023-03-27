using StasDiplom.Domain;
using StasDiplom.Dto.Project.Requests;
using StasDiplom.Dto.Project.Responses;
using StasDiplom.Dto.Task;
using StasDiplom.Dto.Users;
using StasDiplom.Dto.Users.Project;
using StasDiplom.Dto.Users.Task;
using Task = System.Threading.Tasks.Task;
using DomainTask = StasDiplom.Domain.Task;
namespace StasDiplom.Services.Interfaces;

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