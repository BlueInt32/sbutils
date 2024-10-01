# SbuTils
Nuget package: https://www.nuget.org/packages/SbuTils/
## Aspnet Exception handling
1. Create an "error code enum" that will list all exception errors in your app.
```csharp
public enum ErrorCodeEnum
{
    ITEM_NOT_FOUND,
    GROUP_ITEM_NOT_FOUND,
    ITEM_FILE_NOT_FOUND
    /* ... */
}

```

2. Create a project "umbrella" exception e.g. `MyMainException` that inherits `Exception` and could be inherited itself if needed. This exception must implement `IHaveErrorCode` returning the enum value which can be resolved generically from this exception.

3. In aspnet core Program.cs, use the Exception middleware like so: 

``` csharp

app.UseExceptionMiddleware(
    new ExceptionMiddlewareOptions<ErrorCodeEnum, MyMainException>
    {
        UnhandledErrorEnumValue = ErrorCodeEnum.UNHANDLED,
        EnumToStatusCodeMap = new Dictionary<ErrorCodeEnum, HttpStatusCode>
        {
            // 400s
            { ErrorCodeEnum.ITEM_NOT_FOUND, 404 },
            { ErrorCodeEnum.GROUP_ITEM_NOT_FOUND, 404 },
            { ErrorCodeEnum.TARGET_DIRECTORY_DOES_NOT_EXIST, 400 },
            { ErrorCodeEnum.ITEM_WITH_SAME_PATH_ALREADY_EXISTS, 400 },
            { ErrorCodeEnum.TAG_ALREADY_EXISTS, 400 },
            { ErrorCodeEnum.NEW_RELATIVE_PATH_IS_THE_SAME, 400 },
            { ErrorCodeEnum.UNKNOWN_FILE_EXTENSION, 400 },
            { ErrorCodeEnum.DUPLICATE_CATALOG_PATH, 400 },
            { ErrorCodeEnum.CATALOG_DOES_NOT_EXIST, 400 },

            // 500s
            { ErrorCodeEnum.ITEM_FILE_NOT_FOUND, 500 },
            { ErrorCodeEnum.CANNOT_MOVE_ITEM_FILE, 500 },
            { ErrorCodeEnum.FFMPEG_PROBING_FAILURE, 500 },
            { ErrorCodeEnum.FFMPEG_OR_IMAGEMAGICK_FAILURE, 500 },
        }
    }
);


```
