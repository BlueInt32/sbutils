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

    public Result<T, TErrorCodeEnum> OnError(Action<ResultError<TErrorCodeEnum>> thrower)
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

    public new Result<T, TErrorCodeEnum> ThrowOnError<TException>()
        where TException : Exception, IConvertFromNotOkResultException<TErrorCodeEnum>
    {
        if (!Success)
        {
            TException ex = (TException)
                Activator.CreateInstance(typeof(TException), this.Error.Message);
            ex.Code = this.Error.Code;
            throw ex;
        }
        return this;
    }

    public T? ExtractObject()
    {
        return Obj;
    }
}
