namespace AlfaCRM.Domain.Models.DTOs;

public record AdminStatisticsDTO(
    int DepartmentsAmount,
    int UsersAmount,
    int ProblemCasesCount,
    int SolvedProblemCasesCount,
    int SuggestionsCount,
    int SolvedSuggestionsCount
    );