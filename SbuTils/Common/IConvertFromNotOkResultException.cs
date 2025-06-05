namespace SbuTils.Common
{
    public interface IConvertFromNotOkResultException<TErrorCodeEnum>
        : IHaveErrorCode<TErrorCodeEnum>
        where TErrorCodeEnum : struct, IConvertible
    {
        string Message { get; }
    }
}
