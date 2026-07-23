namespace MikroProje.Application.Common.Results;

public class Result<T>
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;

    public T? Data { get; init; }

    public int StatusCode { get; init; }

    public static Result<T> Ok(T? data, string message = "Success")
    {
        return new Result<T>
        {
            Success = true,
            Message = message,
            Data = data,
            StatusCode = 200
        };
    }

    public static Result<T> Created(T? data, string message = "Created")
    {
        return new Result<T>
        {
            Success = true,
            Message = message,
            Data = data,
            StatusCode = 201
        };
    }

    public static Result<T> NoContent(string message = "No content")
    {
        return new Result<T>
        {
            Success = true,
            Message = message,
            Data = default,
            StatusCode = 204
        };
    }

    public static Result<T> Fail(string message, int statusCode)
    {
        return new Result<T>
        {
            Success = false,
            Message = message,
            Data = default,
            StatusCode = statusCode
        };
    }
}
