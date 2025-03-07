namespace SbuTils.Common;

public class Result<T, TErrorCodeEnum>
    where T : class
    where TErrorCodeEnum : struct, IConvertible
{
    public static Result<T, TErrorCodeEnum> Ok(T obj)
    {
        return new Result<T, TErrorCodeEnum> { Obj = obj, Success = true };
    }

    public static Result<T, TErrorCodeEnum> NotOk(
        TErrorCodeEnum errorCode,
        string errorMessage = "",
        object? data = null
    )
    {
        return new Result<T, TErrorCodeEnum>
        {
            Error = new ResultError<TErrorCodeEnum>
            {
                Code = errorCode,
                Message = errorMessage,
                Data = data
            },
            Success = false
        };
    }

    public T? Obj { get; set; }
    public bool Success { get; set; }
    public ResultError<TErrorCodeEnum>? Error { get; set; }

    public Result<T, TErrorCodeEnum> ThrowOnError(Action<ResultError<TErrorCodeEnum>> thrower)
    {
        if (!Success)
        {
            if (Error == null || Error.Code.Equals(default(TErrorCodeEnum)))
            {
                throw new InvalidOperationException(
                    "Cannot throw if Result's Error is null or does not contain at least a Code"
                );
            }
            thrower(Error);
            // throw new TException( { ErrorCode = Error!.Code, Message = Error!.Message };
        }
        return this;
    }
}

public class ResultError<TErrorCodeEnum>
{
    /// <summary>
    /// Error describing succintely the error, e.g. CERTIFICATION_NOT_FOUND
    /// </summary>
    public TErrorCodeEnum? Code { get; set; }

    /// <summary>
    /// More human readable details about the error
    /// </summary>
    public string? Message { get; set; }

    public object? Data { get; set; }
}

public class Result<TErrorCodeEnum> : Result<object, TErrorCodeEnum>
    where TErrorCodeEnum : struct, IConvertible
{
    public static new Result<TErrorCodeEnum> Ok
    {
        get { return new Result<TErrorCodeEnum> { Obj = null, Success = true }; }
    }

    public static new Result<TErrorCodeEnum> NotOk(
        TErrorCodeEnum errorCode,
        string errorMessage = "",
        object? data = null
    )
    {
        return new Result<TErrorCodeEnum>
        {
            Error = new ResultError<TErrorCodeEnum>
            {
                Code = errorCode,
                Message = errorMessage,
                Data = data
            },
            Success = false
        };
    }

    public new Result<TErrorCodeEnum> ThrowOnError(Action<ResultError<TErrorCodeEnum>> thrower)
    {
        if (!Success)
        {
            if (Error == null || Error.Code.Equals(default(TErrorCodeEnum)))
            {
                throw new InvalidOperationException(
                    "Cannot throw if Result's Error is null or does not contain at least a Code"
                );
            }
            thrower(Error);
        }
        return this;
    }
}
