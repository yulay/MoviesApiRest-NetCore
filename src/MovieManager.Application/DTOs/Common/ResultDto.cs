namespace MovieManager.Application.DTOs.Common;

public class ResultDto<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ResultDto<T> SuccessResult(T data, string? message = null)
    {
        return new ResultDto<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static ResultDto<T> FailureResult(string error)
    {
        return new ResultDto<T>
        {
            Success = false,
            Errors = new List<string> { error }
        };
    }

    public static ResultDto<T> FailureResult(List<string> errors)
    {
        return new ResultDto<T>
        {
            Success = false,
            Errors = errors
        };
    }
}
