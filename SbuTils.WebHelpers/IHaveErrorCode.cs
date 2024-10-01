namespace SbuTils.WebHelpers;

public interface IHaveErrorCode<TErrorCodeEnum>
    where TErrorCodeEnum : struct, IConvertible
{
    public TErrorCodeEnum ErrorCode { get; set; }
}
