namespace ParkGate.Application.Common;

/// <summary>
/// Represents the result of an operation that can succeed or fail with an Error.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
        {
            throw new InvalidOperationException("A success result cannot have an error.");
        }

        if (!isSuccess && (error == Error.None || error.IsEmpty))
        {
            throw new InvalidOperationException("A failure result must specify an error.");
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result Failure(string code, string message = "") => new(false, new Error(code, message));

    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result<T> Failure<T>(Error error) => Result<T>.Failure(error);
    public static Result<T> Failure<T>(string code, string message = "") => Result<T>.Failure(new Error(code, message));

    public static implicit operator Result(Error error) => Failure(error);
}

/// <summary>
/// Represents the result of an operation producing a value of type T or failing with an Error.
/// </summary>
/// <typeparam name="T">The type of the successful value.</typeparam>
public sealed class Result<T> : Result
{
    private readonly T? _value;

    private Result(T value) : base(true, Error.None)
    {
        _value = value;
    }

    private Result(Error error) : base(false, error)
    {
        _value = default;
    }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException($"Cannot access Value on a failure result. Error: {Error.Code}");

    public static Result<T> Success(T value) => new(value);
    public new static Result<T> Failure(Error error) => new(error);
    public new static Result<T> Failure(string code, string message = "") => new(new Error(code, message));

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure(error);
}
