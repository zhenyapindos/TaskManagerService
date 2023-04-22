using TaskService.Domain;
using TaskService.Dto.Project;
using TaskService.Dto.Project.Requests;
using TaskService.Dto.Project.Responses;
using TaskService.Dto.Users;
using TaskService.Dto.Users.Project;
using Task = System.Threading.Tasks.Task;

namespace TaskService.Services.Interfaces;

public interface IProjectService
{
    public Task<Project> CreateProject(CreateProjectRequest createProjectRequest, string id);
    public IEnumerable<UserProjectResponse> GetAllProjects(string id);
    public Task<ICollection<ProjectsUsersTasks>> GetUsersTasks(string id);
    public Task<(Project project, User user)> InviteUser(UserProjectInteractionRequest request, string userId);
    public Task<(Project project, User user)> KickUser(UserProjectInteractionRequest request, string userId);
    public Task ChangeRole(UserProjectInteractionRequest request, string userId);
    public Task<UpdateProjectResponse> UpdateProject(UpdateProjectRequest request, string userId);
    public Task DeleteProject(DeleteRequest request, string userId);
    public Task<GetProjectResponse> GetProject(int projectId, string userId);
    public Task AcceptInvitation(int projectId, string userId);
    public IEnumerable<UserShortInfo> GetProjectUsers(int projectId, string userId);
}