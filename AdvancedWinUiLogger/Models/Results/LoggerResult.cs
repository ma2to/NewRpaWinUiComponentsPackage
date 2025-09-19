namespace RpaWinUiComponentsPackage.AdvancedWinUiLogger.Models.Results;

/// <summary>
/// 🎯 PUBLIC RESULT TYPE: Clean API result wrapper for logger operations
/// FUNCTIONAL: Monadic result pattern for composable error handling
/// IMMUTABLE: Safe result representation
/// </summary>
public sealed record LoggerResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }

    private LoggerResult(bool isSuccess, string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// FUNCTIONAL: Create successful result
    /// </summary>
    public static LoggerResult Success() => new(true, null);

    /// <summary>
    /// FUNCTIONAL: Create failure result
    /// </summary>
    public static LoggerResult Failure(string error) => new(false, error);

    /// <summary>
    /// FUNCTIONAL: Execute action if successful
    /// </summary>
    public LoggerResult OnSuccess(Action action)
    {
        if (IsSuccess)
        {
            try
            {
                action();
            }
            catch
            {
                // Don't let side effects break the result chain
            }
        }
        return this;
    }

    /// <summary>
    /// FUNCTIONAL: Execute action if failed
    /// </summary>
    public LoggerResult OnFailure(Action<string> action)
    {
        if (!IsSuccess && ErrorMessage != null)
        {
            try
            {
                action(ErrorMessage);
            }
            catch
            {
                // Ignore exceptions in error handling
            }
        }
        return this;
    }
}

/// <summary>
/// 🎯 PUBLIC RESULT TYPE: Generic result wrapper with value
/// FUNCTIONAL: Monadic result pattern with typed return values
/// IMMUTABLE: Safe result representation with value
/// </summary>
public sealed record LoggerResult<T>
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public T? Value { get; init; }

    private LoggerResult(bool isSuccess, T? value, string? errorMessage)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// FUNCTIONAL: Create successful result with value
    /// </summary>
    public static LoggerResult<T> Success(T value) => new(true, value, null);

    /// <summary>
    /// FUNCTIONAL: Create failure result
    /// </summary>
    public static LoggerResult<T> Failure(string error) => new(false, default, error);

    /// <summary>
    /// FUNCTIONAL: Map successful value to new type
    /// </summary>
    public LoggerResult<TOut> Map<TOut>(Func<T, TOut> func)
    {
        if (!IsSuccess || Value == null)
            return LoggerResult<TOut>.Failure(ErrorMessage ?? "Value is null");

        try
        {
            var result = func(Value);
            return LoggerResult<TOut>.Success(result);
        }
        catch (Exception ex)
        {
            return LoggerResult<TOut>.Failure($"Map operation failed: {ex.Message}");
        }
    }

    /// <summary>
    /// FUNCTIONAL: Bind operation for chaining
    /// </summary>
    public LoggerResult<TOut> Bind<TOut>(Func<T, LoggerResult<TOut>> func)
    {
        if (!IsSuccess || Value == null)
            return LoggerResult<TOut>.Failure(ErrorMessage ?? "Value is null");

        try
        {
            return func(Value);
        }
        catch (Exception ex)
        {
            return LoggerResult<TOut>.Failure($"Bind operation failed: {ex.Message}");
        }
    }

    /// <summary>
    /// FUNCTIONAL: Get value or default
    /// </summary>
    public T ValueOr(T defaultValue) => IsSuccess && Value != null ? Value : defaultValue;

    /// <summary>
    /// FUNCTIONAL: Execute action on success
    /// </summary>
    public LoggerResult<T> OnSuccess(Action<T> action)
    {
        if (IsSuccess && Value != null)
        {
            try
            {
                action(Value);
            }
            catch
            {
                // Don't let side effects break the result chain
            }
        }
        return this;
    }

    /// <summary>
    /// FUNCTIONAL: Execute action on failure
    /// </summary>
    public LoggerResult<T> OnFailure(Action<string> action)
    {
        if (!IsSuccess && ErrorMessage != null)
        {
            try
            {
                action(ErrorMessage);
            }
            catch
            {
                // Ignore exceptions in error handling
            }
        }
        return this;
    }
}