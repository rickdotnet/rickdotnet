﻿namespace RickDotNet.Base;

public struct Unit
{
    public static readonly Unit Default = new();
}

public static class Result
{
    private static readonly Result<Unit> success = new Result<Unit>.Success(Unit.Default);
    private static readonly Task<Result<Unit>> successTask = Task.FromResult(success);

    public static Result<Unit> Success() => success;
    public static Task<Result<Unit>> SuccessTask() => successTask;
    public static Result<Unit> Error(string error) => new Result<Unit>.Error(error);
    public static Task<Result<Unit>> ErrorTask(string error) => Task.FromResult(Error(error));
    public static Result<Unit> Failure(Exception failure) => new Result<Unit>.Failure(failure);
    public static Result<T> Success<T>(T value) => new Result<T>.Success(value);
    public static Task<Result<T>> SuccessTask<T>(T value) => Task.FromResult(Success(value));
    public static Result<T> Error<T>(string errorMessage) => new Result<T>.Error(errorMessage);
    public static Result<T> Error<T>(Exception failure) => new Result<T>.Error(failure.Message);
    public static Task<Result<T>> ErrorTask<T>(string error) => Task.FromResult(Error<T>(error));
    public static Result<T> Failure<T>(Exception failure) => new Result<T>.Failure(failure);
    public static Task<Result<T>> FailureTask<T>(Exception failure) => Task.FromResult(Failure<T>(failure));

    public static Result<Unit> Try(Action action)
    {
        try
        {
            action();
            return Success();
        }
        catch (Exception e)
        {
            return Failure(e);
        }
    }

    public static Result<Unit> Try(Action action, string errorMessage)
    {
        try
        {
            action();
            return Success();
        }
        catch
        {
            return Error(errorMessage);
        }
    }

    public static async Task<Result<Unit>> TryAsync(Func<Task> func)
    {
        try
        {
            await func();
            return Success();
        }
        catch (Exception e)
        {
            return Failure(e);
        }
    }

    public static async Task<Result<Unit>> TryAsync(Func<Task> func, string errorMessage)
    {
        try
        {
            await func();
            return Success();
        }
        catch
        {
            return Error(errorMessage);
        }
    }


    public static Result<T> Try<T>(Func<T> func)
    {
        try
        {
            return Success(func());
        }
        catch (Exception e)
        {
            return Failure<T>(e);
        }
    }

    public static Result<T> Try<T>(Func<T> func, string errorMessage)
    {
        try
        {
            return Success(func());
        }
        catch
        {
            return Error<T>(errorMessage);
        }
    }

    public static async Task<Result<T>> TryAsync<T>(Func<Task<T>> func)
    {
        try
        {
            return Success(await func());
        }
        catch (Exception e)
        {
            return Failure<T>(e);
        }
    }

    public static async Task<Result<T>> TryAsync<T>(Func<Task<T>> func, string errorMessage)
    {
        try
        {
            return Success(await func());
        }
        catch
        {
            return Error<T>(errorMessage);
        }
    }

    public static Result<T> Try<T>(Func<Result<T>> func)
    {
        try
        {
            return func();
        }
        catch (Exception e)
        {
            return Failure<T>(e);
        }
    }

    public static Result<T> Try<T>(Func<Result<T>> func, string errorMessage)
    {
        try
        {
            return func();
        }
        catch
        {
            return Error<T>(errorMessage);
        }
    }

    public static async Task<Result<T>> TryAsync<T>(Func<Task<Result<T>>> func)
    {
        try
        {
            return await func();
        }
        catch (Exception e)
        {
            return Failure<T>(e);
        }
    }

    public static async Task<Result<T>> TryAsync<T>(Func<Task<Result<T>>> func, string errorMessage)
    {
        try
        {
            return await func();
        }
        catch
        {
            return Error<T>(errorMessage);
        }
    }
}

public abstract record Result<T>
{
    public bool Successful => this is Success;
    public bool NotSuccessful => !Successful;

    /// <summary>
    /// Tell Houston. The result is a <see cref="Result{T}.Failure"/>, aka, an exception occurred during the operation.
    /// </summary>
    public bool BlewUp => this is Failure;

    /// <summary>
    /// Returns the value of the result if it is a <see cref="Result{T}.Success"/>, otherwise returns the default value of type T.
    /// </summary>
    /// <returns>The result value if successful, otherwise the default value of type T.</returns>
    public T? ValueOrDefault() => this is Success success ? success.Value : default;

    /// <summary>
    /// Returns the value of the result if it is a <see cref="Result{T}.Success"/>, otherwise returns the specified default value.
    /// </summary>
    /// <param name="defaultValue">The default value to return if the result is not a <see cref="Result{T}.Success"/>.</param>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <returns>The result value if successful, otherwise the <paramref name="defaultValue"/>.</returns>
    public T ValueOrDefault(T defaultValue) => this is Success success ? success.Value : defaultValue;

    public sealed record Success(T Value) : Result<T>;

    public sealed record Error(string ErrorMessage) : Result<T>;

    public sealed record Failure(Exception Exception) : Result<T>;

    public static implicit operator Result<T>(T value)
        => new Success(value);

    public static implicit operator Result<T>(Exception ex)
        => new Failure(ex);

    public static implicit operator bool(Result<T> result)
        => result is Success;

    public override string ToString() =>
        this switch
        {
            Success success => $"Success: {success.Value}",
            Error error => $"Error: {error}",
            Failure exception => $"Failure: {exception.Exception.Message}",
            _ => "Unknown Result"
        };
}
