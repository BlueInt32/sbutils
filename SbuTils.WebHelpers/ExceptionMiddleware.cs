using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SbuTils.WebHelpers;

public class ExceptionMiddleware<TErrorCodeEnum, TException>
    where TErrorCodeEnum : struct, IConvertible
    where TException : class, IHaveErrorCode<TErrorCodeEnum>
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware<TErrorCodeEnum, TException>> _logger;
    private readonly ExceptionMiddlewareOptions<TErrorCodeEnum, TException> _options;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware<TErrorCodeEnum, TException>> logger,
        ExceptionMiddlewareOptions<TErrorCodeEnum, TException> options
    )
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger;
        _options = options;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            var statusCode = HttpStatusCode.InternalServerError;
            var functionalErrorCode = _options.UnhandledErrorEnumValue;
            var message = ex.Message ?? "Unhandled exception occured";
            var stackTrace = ex.StackTrace ?? "No stack trace available";
            if (ex != null && ex is TException)
            {
                functionalErrorCode = (ex as TException)!.ErrorCode;
                _options.EnumToStatusCodeMap.TryGetValue(functionalErrorCode, out statusCode);
            }
            if (context.Response.HasStarted)
                throw;

            context.Response.Clear();
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";
            var json = new
            {
                Code = functionalErrorCode,
                Message = message,
#if DEBUG
                stackTrace
#endif
            };
            _logger.LogError(
                $"{(int)statusCode} {statusCode} error generated from exception: {message} \n\t{stackTrace}"
            );
            await context.Response.WriteAsJsonAsync(
                json,
                // microsoft.json.text options for enum convertion
                options: new System.Text.Json.JsonSerializerOptions
                {
                    Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
                }
            );
        }
    }
}

public static class ExceptionMiddlewareExtensions
{
    /// <summary>
    /// Register a generic exception handling middleware resolving code and http status from your exception automatically
    /// </summary>
    /// <typeparam name="TErrorCodeEnum"></typeparam>
    /// <typeparam name="TException"></typeparam>
    /// <param name="builder"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IApplicationBuilder UseExceptionMiddleware<TErrorCodeEnum, TException>(
        this IApplicationBuilder builder,
        ExceptionMiddlewareOptions<TErrorCodeEnum, TException> options
    )
        where TErrorCodeEnum : struct, IConvertible
        where TException : class, IHaveErrorCode<TErrorCodeEnum>
    {
        if (options.EnumToStatusCodeMap == null)
        {
            throw new InvalidOperationException("Enum to HttpStatusCode map is mandatory");
        }
        return builder.UseMiddleware<ExceptionMiddleware<TErrorCodeEnum, TException>>(options);
    }
}

public class ExceptionMiddlewareOptions<TErrorCodeEnum, TException>
    where TErrorCodeEnum : struct, IConvertible
    where TException : class, IHaveErrorCode<TErrorCodeEnum>
{
    public TErrorCodeEnum UnhandledErrorEnumValue { get; set; }
    public Dictionary<TErrorCodeEnum, HttpStatusCode> EnumToStatusCodeMap { get; set; } =
        new Dictionary<TErrorCodeEnum, HttpStatusCode>();
}
