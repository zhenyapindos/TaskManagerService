﻿using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StasDiplom.Context;
using StasDiplom.Domain;
using StasDiplom.Dto.Project;
using StasDiplom.Dto.Project.Requests;
using StasDiplom.Dto.Project.Responses;
using StasDiplom.Dto.Task;
using StasDiplom.Dto.Users;
using StasDiplom.Dto.Users.Project;
using StasDiplom.Enum;
using StasDiplom.Services.Interfaces;
using Task = System.Threading.Tasks.Task;

namespace StasDiplom.Services;

public class ProjectService : IProjectService
{
    private readonly ProjectManagerContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;

    public ProjectService(ProjectManagerContext context, UserManager<User> userManager, IMapper mapper)
    {
        _context = context;
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<Project> CreateProject(CreateProjectRequest createProjectRequest, string id)
    {
        var newProject = _mapper.Map<Project>(createProjectRequest);

        await _context.Projects.AddAsync(newProject);

        await _context.SaveChangesAsync();

        var projectUser = new ProjectUser
        {
            UserId = id,
            UserProjectRole = UserProjectRole.Admin,
            Project = newProject
        };

        await _context.ProjectUsers.AddAsync(projectUser);

        await _context.SaveChangesAsync();

        return newProject;
    }

    public IEnumerable<UserProjectResponse> GetAllProjects(string id)
    {
        var projectUsers = _mapper.Map<IEnumerable<UserProjectResponse>>(_context
            .ProjectUsers
            .AsNoTracking()
            .Where(x => x.UserId == id)
            .Include(x => x.Project));

        return projectUsers;
    }

    public async Task<ICollection<ProjectsUsersTasks>> GetUsersTasks(string id)
    {
        var projects = _context.Projects
            .Include(x=> x.ProjectUsers)
            .ThenInclude(x=> x.User)
            .Include(x => x.Tasks)
            .ThenInclude(x => x.TaskUsers)
            .ToList();

        var response = new List<ProjectsUsersTasks>();
        
        foreach (var project in projects)
        {
            var currentProjectUser = project.ProjectUsers.FirstOrDefault(x => x.UserId == id);
            if (!project.ProjectUsers.Contains(currentProjectUser!))
            {
                continue;
            }
            var responseElement = new ProjectsUsersTasks
            {
                ProjectId = project.Id,
                Title = project.Title,
                TaskList = new List<TaskShortInfo>()
            };

            foreach (var task in project.Tasks)
            {
                var currentTaskUser = task.TaskUsers.FirstOrDefault(x => x.UserId == id);

                if (!task.TaskUsers.Contains(currentTaskUser!))
                {
                    continue;
                }
                
                if (task.TaskUsers.Contains(currentTaskUser!))
                {
                    responseElement.TaskList.Add(_mapper.Map<TaskShortInfo>(task));
                }
            }

            response.Add(responseElement);
        }

        return response;
    }

    public async Task<(Project, User)> InviteUser(UserProjectInteractionRequest request, string userId)
    {
        var user = request.EmailOrUsername.Contains('@')
            ? await _userManager.FindByEmailAsync(request.EmailOrUsername)
            : await _userManager.FindByNameAsync(request.EmailOrUsername);

        if (user == null)
        {
            throw new ArgumentException("User is not exist");
        }

        var project = _context.Projects
            .Include(x => x.Notifications)
            .FirstOrDefault(x => x.Id == request.ProjectId);

        if (project == null)
        {
            throw new ArgumentException("Project is not exist");
        }

        var resultUser = _context.ProjectUsers
            .AsNoTracking()
            .FirstOrDefault(x => x.UserId == userId && x.ProjectId == project.Id);

        if (resultUser == null)
        {
            throw new ArgumentException("User has no permissions");
        }

        var projectUser = new ProjectUser
        {
            UserProjectRole = UserProjectRole.Invited,
            User = user,
            Project = project
        };

        await _context.ProjectUsers.AddAsync(projectUser);
        await _context.SaveChangesAsync();

        return (project, user);
    }

    public async Task<(Project project, User user)> KickUser(UserProjectInteractionRequest request, string userId)
    {
        var user = request.EmailOrUsername.Contains('@')
            ? await _userManager.FindByEmailAsync(request.EmailOrUsername)
            : await _userManager.FindByNameAsync(request.EmailOrUsername);

        if (user == null)
        {
            throw new ArgumentException("User is not exist");
        }

        var project = _context.Projects.Include(x => x.ProjectUsers).FirstOrDefault(x => x.Id == request.ProjectId);

        if (project == null)
        {
            throw new ArgumentException("Project is not exist");
        }

        var resultUser = project.ProjectUsers.FirstOrDefault(x => x.UserId == userId);

        if (resultUser == null)
        {
            throw new ArgumentException("User has no permissions");
        }

        resultUser = project.ProjectUsers.FirstOrDefault(x => x.UserId == user.Id);

        if (resultUser == null)
        {
            throw new ArgumentException("User is not in project");
        }

        _context.ProjectUsers.Remove(resultUser);
        await _context.SaveChangesAsync();

        return (project, user);
    }

    public async Task ChangeRole(UserProjectInteractionRequest request, string userId)
    {
        var user = request.EmailOrUsername.Contains('@')
            ? await _userManager.FindByEmailAsync(request.EmailOrUsername)
            : await _userManager.FindByNameAsync(request.EmailOrUsername);

        if (user == null)
        {
            throw new ArgumentException("User is not exist");
        }

        var project = _context.Projects
            .Include(x => x.ProjectUsers)
            .FirstOrDefault(x => x.Id == request.ProjectId);

        if (project == null)
        {
            throw new ArgumentException("Project is not exist");
        }

        var resultUser = project.ProjectUsers.FirstOrDefault(x => x.UserId == userId);

        if (resultUser == null)
        {
            throw new ArgumentException("User has no permissions");
        }

        resultUser = project.ProjectUsers.FirstOrDefault(x => x.UserId == user.Id);

        if (resultUser == null)
        {
            throw new ArgumentException("User is not in project");
        }

        resultUser.UserProjectRole = resultUser.UserProjectRole == UserProjectRole.Worker
            ? UserProjectRole.Moderator
            : UserProjectRole.Worker;

        _context.ProjectUsers.Update(resultUser);
        await _context.SaveChangesAsync();
    }

    public async Task<UpdateProjectResponse> UpdateProject(UpdateProjectRequest request, string userId)
    {
        var project = _context.Projects
            .Include(x => x.ProjectUsers)
            .FirstOrDefault(y => y.Id == request.Id);

        if (project == null)
        {
            throw new ArgumentException("Project is not exist");
        }

        var resultUser = project.ProjectUsers
            .FirstOrDefault(x => x.UserId == userId && x.UserProjectRole == UserProjectRole.Admin);

        if (resultUser == null)
        {
            throw new ArgumentException("User has no permissions");
        }

        if (request.Title != null)
        {
            project.Title = request.Title;
        }

        if (request.Description != null)
        {
            project.Description = request.Description;
        }

        _context.Projects.Update(project);
        await _context.SaveChangesAsync();

        return new UpdateProjectResponse
        {
            Title = project.Title,
            Description = project.Description
        };
    }

    public async Task DeleteProject(DeleteRequest request, string userId)
    {
        var project = _context.Projects
            .Include(x => x.ProjectUsers)
            .FirstOrDefault(y => y.Id == request.Id);

        if (project == null)
        {
            throw new ArgumentException("Project is not exist");
        }

        var resultUser = project.ProjectUsers
            .FirstOrDefault(x => x.UserId == userId && x.UserProjectRole == UserProjectRole.Admin);

        if (resultUser == null)
        {
            throw new ArgumentException("User has no permissions");
        }

        _context.Remove(project);
        await _context.SaveChangesAsync();
    }

    public async Task<GetProjectResponse> GetProject(int projectId, string userId)
    {
        var project = _context.Projects.Include(x => x.ProjectUsers)
            .Include(x => x.Tasks)
            .FirstOrDefault(x => x.Id == projectId);

        if (project == null)
        {
            throw new ArgumentException("Project is not exist");
        }

        var response = _mapper.Map<GetProjectResponse>(project);

        var resultUser = project.ProjectUsers.FirstOrDefault(x => x.UserId == userId);

        if (resultUser == null)
        {
            throw new ArgumentException("User has no permissions");
        }

        var users = project.ProjectUsers.Join(_userManager.Users,
                pu => pu.UserId, u => u.Id, (projectUser, user) => _mapper.Map<UserShortInfo>((user, projectUser)))
            .ToList();

        response.UserList = users;
        response.UserProjectRole = resultUser.UserProjectRole;
        response.TaskList = new List<TaskShortInfo>();

        foreach (var task in project.Tasks)
        {
            response.TaskList.Add(_mapper.Map<TaskShortInfo>(task));
        }

        return response;
    }

    public async Task AcceptInvitation(int projectId, string userId)
    {
        var project = _context.Projects
            .Include(x => x.ProjectUsers)
            .FirstOrDefault(y => y.Id == projectId);

        if (project == null)
        {
            throw new ArgumentException("Project is not exist");
        }

        var resultUser = project.ProjectUsers
            .FirstOrDefault(x => x.UserId == userId);

        if (resultUser == null)
        {
            throw new ArgumentException("User is already on project");
        }

        if (resultUser.UserProjectRole != UserProjectRole.Invited)
        {
            throw new ArgumentException("User is not invited on project");
        }

        resultUser.UserProjectRole = UserProjectRole.Worker;

        await _context.SaveChangesAsync();
    }

    public IEnumerable<UserShortInfo> GetProjectUsers(int projectId, string userId)
    {
        var project = _context.Projects
            .Include(x => x.ProjectUsers)
            .FirstOrDefault(x => x.Id == projectId);

        if (project == null)
        {
            throw new ArgumentException("Project is not exist");
        }

        var resultUser = project.ProjectUsers.FirstOrDefault(x => x.UserId == userId);

        if (resultUser == null)
        {
            throw new ArgumentException("User has no permissions");
        }

        var users = project.ProjectUsers.Join(_userManager.Users,
                pu => pu.UserId, u => u.Id, (projectUser, user) => _mapper.Map<UserShortInfo>((user, projectUser)))
            .ToList();

        return users;
    }
}