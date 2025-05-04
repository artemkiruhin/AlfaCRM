using AlfaCRM.Domain.Models.Settings;

namespace AlfaCRM.Api.Contracts.Request;

public record ReportCreateApiRequest(
    string? Title,
    string? Description,
    ReportTableNumber Table
    );