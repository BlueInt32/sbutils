namespace SbuTils.Common;

public interface IHaveErrorCode<TErrorCodeEnum>
    where TErrorCodeEnum : struct, IConvertible
{
    TErrorCodeEnum Code { get; set; }
}
