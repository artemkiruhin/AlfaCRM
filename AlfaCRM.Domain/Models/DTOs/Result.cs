namespace AlfaCRM.Domain.Models.DTOs;

public class Result<T>
{
    private Result() { }

    public bool IsSuccess { get; init; }
    public string ErrorMessage { get; init; }
    public T Data { get; init; }

    public static Result<T> Failure(string errorMessage) => new() { IsSuccess = false, ErrorMessage = errorMessage };
    public static Result<T> Success(T data) => new() { Data = data, IsSuccess = true };
}