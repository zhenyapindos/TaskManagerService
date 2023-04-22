using TaskService.Domain;
using TaskService.Dto.Calendar;
using Task = System.Threading.Tasks.Task;

namespace TaskService.Services.Interfaces;

public interface ICalendarService
{
    Task<CalendarInfo> GetUserCalendar(string username);
    Task<CalendarInfo> GetProjectCalendar(int projectId);
    Task CreateUserCalendar(User user);
    Task CreateProjectCalendar(Project project);
}