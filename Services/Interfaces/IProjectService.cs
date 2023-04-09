using StasDiplom.Domain;
using StasDiplom.Dto.Project;
using StasDiplom.Dto.Project.Requests;
using StasDiplom.Dto.Project.Responses;
using StasDiplom.Dto.Users;
using StasDiplom.Dto.Users.Project;
using Task = System.Threading.Tasks.Task;

namespace StasDiplom.Services.Interfaces;

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