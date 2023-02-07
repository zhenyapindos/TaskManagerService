using StasDiplom.Domain;
using StasDiplom.Enum;

namespace StasDiplom.Extensions;

public class ProjectDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public UserProjectRole UserProjectRole { get; set; }
    
    public static ProjectDto ToProjectDto(Project project)
    {
        var projectDto = new ProjectDto
        {
            Id = project.Id,
            Title = project.Title,
            Description = project.Description
            //???? 
        };

        return projectDto;
    }
}