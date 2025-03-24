namespace SbuTils.Common;

public interface IHaveErrorCode<TErrorCodeEnum>
    where TErrorCodeEnum : struct, IConvertible
{
    public TErrorCodeEnum Code { get; set; }
}
