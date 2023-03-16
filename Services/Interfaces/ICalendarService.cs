using StasDiplom.Domain;
using StasDiplom.Dto.Calendar;
using Task = System.Threading.Tasks.Task;

namespace StasDiplom.Services.Interfaces;

public interface ICalendarService
{
    Task<CalendarInfo> GetUserCalendar(string username);
    Task<CalendarInfo> GetProjectCalendar(int projectId);
    Task CreateUserCalendar(User user);
    Task CreateProjectCalendar(Project project);
}