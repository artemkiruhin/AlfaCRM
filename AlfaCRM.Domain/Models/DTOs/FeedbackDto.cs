﻿namespace AlfaCRM.Domain.Models.DTOs;

public record FeedbackDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Content { get; init; }
    public string? Email { get; init; }
    public required string Type { get; init; }
    public DateTime CreatedAt { get; init; }
}