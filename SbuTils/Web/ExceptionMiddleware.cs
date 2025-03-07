using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SbuTils.Common;

namespace SbuTils.Web;

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
            var statusCode = (int)HttpStatusCode.InternalServerError;
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
                    Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
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
        // important: check if all enum values are present in the map, otherwise throw
        TErrorCodeEnum[] actualErrorCode = (TErrorCodeEnum[])Enum.GetValues(typeof(TErrorCodeEnum));
        var notHandledEnumErrorCodes = actualErrorCode.Where(
            code => !options.EnumToStatusCodeMap.ContainsKey(code)
        );
        if (notHandledEnumErrorCodes.Any())
        {
            // if the only not handled code is the configured unhandled, branch out, otherwise, throw
            // Mysterious unable to compare TErrorCodeEnum values, had to ToString() to compare...
            if (
                notHandledEnumErrorCodes.Count() != 1
                || notHandledEnumErrorCodes.First().ToString()
                    != options.UnhandledErrorEnumValue.ToString()
            )
            {
                throw new InvalidOperationException(
                    $"All Enum's codes must have a corresponding key in the map. I know, tough right? ;). "
                        + $"Here is the list of not handled codes: {string.Join(',', notHandledEnumErrorCodes)}"
                );
            }
        }

        return builder.UseMiddleware<ExceptionMiddleware<TErrorCodeEnum, TException>>(options);
    }
}

public class ExceptionMiddlewareOptions<TErrorCodeEnum, TException>
    where TErrorCodeEnum : struct, IConvertible
    where TException : class, IHaveErrorCode<TErrorCodeEnum>
{
    public TErrorCodeEnum UnhandledErrorEnumValue { get; set; }
    public Dictionary<TErrorCodeEnum, int> EnumToStatusCodeMap { get; set; } =
        new Dictionary<TErrorCodeEnum, int>();
}
