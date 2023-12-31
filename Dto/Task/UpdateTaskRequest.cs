﻿namespace TaskService.Dto.Task;

public record UpdateTaskRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public double? DurationHours { get; set; }
}