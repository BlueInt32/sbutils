namespace SbuTils.Common;

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

    public new Result<TErrorCodeEnum> OnError(Action<ResultError<TErrorCodeEnum>> thrower)
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

    public new Result<TErrorCodeEnum> ThrowOnError<TException>()
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
}
