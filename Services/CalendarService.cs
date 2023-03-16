using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StasDiplom.Context;
using StasDiplom.Domain;
using StasDiplom.Dto;
using StasDiplom.Dto.Calendar;
using StasDiplom.Services.Interfaces;
using Task = System.Threading.Tasks.Task;

namespace StasDiplom.Services;

public class CalendarService : ICalendarService
{
    private readonly ProjectManagerContext _context;
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;

    public CalendarService(ProjectManagerContext context, IMapper mapper, UserManager<User> userManager)
    {
        _context = context;
        _mapper = mapper;
        _userManager = userManager;
    }

    public async Task<CalendarInfo> GetUserCalendar(string username)
    {
        var user = _userManager.FindByNameAsync(username).Result;
        var calendar = _context.Calendars.Include(x => x.Events).FirstOrDefault(x => x.UserId == user.Id);

        var mappedCalendar = _mapper.Map<CalendarInfo>(calendar) with
        {
            Username = username
        };
        
        foreach (var eventInfo in mappedCalendar.Events)
        {
            eventInfo.CreatorUsername = username;
        }

        return mappedCalendar;
    }

    public async Task<CalendarInfo> GetProjectCalendar(int projectId)
    {
        var calendar = _context.Calendars
            .Include(x => x.Project)
            .Include(x => x.Events)
            .ThenInclude(x => x.EventUsers)
            .FirstOrDefault(x => x.Project.Id == projectId);

        var creators = calendar.Events.ToDictionary(x=> x.Id, x => _userManager.FindByIdAsync(x.CreatorId).Result);

        var mappedCalendar = _mapper.Map<CalendarInfo>(calendar) with
        {
            ProjectId = projectId
        };
        
        foreach (var eventInfo in mappedCalendar.Events)
        {
            eventInfo.CreatorUsername = creators[eventInfo.Id].UserName;
        }

        return mappedCalendar;
    }

    public async Task CreateUserCalendar(User user)
    {
        var calendar = new Calendar()
        {
            Title = user.UserName + "'s calendar",
            User = user,
            Events = new List<Event>()
        };

        _context.Calendars.Add(calendar);
        await _context.SaveChangesAsync();
    }

    public async Task CreateProjectCalendar(Project project)
    {
        var calendar = new Calendar
        {
            Title = project.Title + "'s calendar",
            Events = new List<Event>()
        };
        
        calendar.Project = project;

        await _context.Calendars.AddAsync(calendar);
        await _context.SaveChangesAsync();
    }
}