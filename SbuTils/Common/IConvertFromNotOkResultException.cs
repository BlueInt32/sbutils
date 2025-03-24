namespace SbuTils.Common
{
    public interface IConvertFromNotOkResultException<TErrorCodeEnum>
        : IHaveErrorCode<TErrorCodeEnum>
        where TErrorCodeEnum : struct, IConvertible
    {
        TErrorCodeEnum Code { get; set; }
        string Message { get; }
    }
}
