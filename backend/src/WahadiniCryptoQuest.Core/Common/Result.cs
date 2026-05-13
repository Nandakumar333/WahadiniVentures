namespace WahadiniCryptoQuest.Core.Common;

public class Result<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage => Message;

    public static Result<T> Success(T data, string? message = null)
    {
        return new Result<T> { IsSuccess = true, Data = data, Message = message };
    }

    public static Result<T> Failure(string message, string errorCode = "ERROR")
    {
        return new Result<T> { IsSuccess = false, Message = message, ErrorCode = errorCode };
    }
}
