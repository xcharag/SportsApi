namespace SportsApi.domain.Abstractions.Dtos;

public class Result
{
    public bool IsSuccess { get; set; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; set; }
    public string ErrorKey { get; set; }

    protected Result(bool isSuccess, string error, string errorKey = "")
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorKey = errorKey;
    }

    public static Result Success() => new(true, string.Empty);
    public static Result Fail(string message, string errorKey = "") => 
        new(false, message, errorKey);
    
    public static Result<T> Success<T>(T value) => new(value, true, string.Empty);
    public static Result<T> Fail<T>(string message, string errorKey = "") =>
        new(default!, false, message, errorKey);
    
}

public class Result<T> : Result
{
    public T Value { get; set; }
    
    internal Result(T value, bool isSuccess, string error, string errorKey = "") : base(isSuccess, error)
    {
        Value = value;
    }
}