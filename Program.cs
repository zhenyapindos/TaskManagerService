using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StasDiplom.Context;
using StasDiplom.Domain;
using StasDiplom.Dto.Comment;
using StasDiplom.Dto.Project;
using StasDiplom.Dto.Project.Requests;
using StasDiplom.Dto.Project.Responses;
using StasDiplom.Enum;
using Task = StasDiplom.Domain.Task;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddDbContext<ProjectManagerContext>(
    options => options.UseSqlServer(configuration.GetConnectionString("MsSqlServerExpress")));

builder.Services.AddAutoMapper(config =>
{
    config.CreateMap<ProjectUser, UserProjectResponse>(MemberList.Destination)
        .ForMember(
            x => x.Title,
            opt =>
                opt.MapFrom(src => src.Project.Title))
        .ForMember(
            x => x.Description,
            opt =>
                opt.MapFrom(src => src.Project.Description))
        .ForMember(
            x => x.Id,
            opt =>
                opt.MapFrom(src => src.Project.Id));

    config.CreateMap<Task, TaskShortInfo>(MemberList.Destination)
        .ForMember(x => x.Deadline,
            opt =>
                opt.MapFrom(src => src.StartDate.Value.AddHours(src.DurationTime.Value)));

    config.CreateMap<(User user, ProjectUser projectUser), UserProjectInfo>(MemberList.Destination)
        .ForMember(x => x.FirstName,
            opt => 
                opt.MapFrom(src => src.user.FirstName))
        .ForMember(x => x.LastName,
        opt => 
            opt.MapFrom(src => src.user.LastName))
        .ForMember(x => x.Email,
        opt => 
            opt.MapFrom(src => src.user.Email))
        .ForMember(x => x.UserProjectRole,
            opt =>
                opt.MapFrom(src => src.projectUser.UserProjectRole));

    config.CreateMap<Project, GetProjectResponse>(MemberList.Destination);
    
    config.CreateMap<CreateCommentRequest, Comment>(MemberList.Source);
    
    config.CreateMap<Project, CreateProjectResponse>(MemberList.Destination);
    
    config.CreateMap<CreateProjectRequest, Project>(MemberList.Source);
});

builder.Services.AddIdentity<User, IdentityRole>(o =>
    {
        o.Password.RequireDigit = false;
        o.Password.RequireNonAlphanumeric = false;
        o.Password.RequireLowercase = false;
        o.Password.RequireUppercase = false;
        o.Password.RequiredLength = 1;
    })
    .AddEntityFrameworkStores<ProjectManagerContext>()
    .AddDefaultTokenProviders();


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey
            (Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true
    };
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo()
    {
        Title = "Diploma",
        Version = "v1"
    });
    
    c.ResolveConflictingActions(apiDescriptor => apiDescriptor.First());
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme()
            {
                Reference = new OpenApiReference()
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

app.UseRouting();

app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();