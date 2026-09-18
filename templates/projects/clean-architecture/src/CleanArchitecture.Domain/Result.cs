namespace CleanArchitecture.Domain;

public enum ErrorType { None, Validation, NotFound, Conflict }

public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public ErrorType ErrorType { get; }

    protected Result(bool isSuccess, string? error, ErrorType errorType)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorType = errorType;
    }

    public static Result Success() => new(true, null, ErrorType.None);
    public static Result Failure(string error, ErrorType type) => new(false, error, type);
}

public sealed class Result<T> : Result
{
    private readonly T? _value;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access the value of a failed result.");

    private Result(T value) : base(true, null, ErrorType.None) => _value = value;
    private Result(string error, ErrorType type) : base(false, error, type) { }

    public static Result<T> Success(T value) => new(value);
    public static new Result<T> Failure(string error, ErrorType type) => new(error, type);
}
