# SbuTils

In aspnet core Program.cs, use the Exception middleware like so: 

``` csharp

app.UseExceptionMiddleware(
    new ExceptionMiddlewareOptions<ErrorCodeEnum, VStackException>
    {
        UnhandledErrorEnumValue = ErrorCodeEnum.UNHANDLED,
        EnumToStatusCodeMap = new Dictionary<ErrorCodeEnum, HttpStatusCode>
        {
            // 400s
            { ErrorCodeEnum.ITEM_NOT_FOUND, (HttpStatusCode)404 },
            { ErrorCodeEnum.GROUP_ITEM_NOT_FOUND, (HttpStatusCode)404 },
            { ErrorCodeEnum.TARGET_DIRECTORY_DOES_NOT_EXIST, (HttpStatusCode)400 },
            { ErrorCodeEnum.ITEM_WITH_SAME_PATH_ALREADY_EXISTS, (HttpStatusCode)400 },
            { ErrorCodeEnum.TAG_ALREADY_EXISTS, (HttpStatusCode)400 },
            { ErrorCodeEnum.NEW_RELATIVE_PATH_IS_THE_SAME, (HttpStatusCode)400 },
            { ErrorCodeEnum.UNKNOWN_FILE_EXTENSION, (HttpStatusCode)400 },
            { ErrorCodeEnum.DUPLICATE_CATALOG_PATH, (HttpStatusCode)400 },
            { ErrorCodeEnum.CATALOG_DOES_NOT_EXIST, (HttpStatusCode)400 },

            // 500s
            { ErrorCodeEnum.ITEM_FILE_NOT_FOUND, (HttpStatusCode)500 },
            { ErrorCodeEnum.CANNOT_MOVE_ITEM_FILE, (HttpStatusCode)500 },
            { ErrorCodeEnum.FFMPEG_PROBING_FAILURE, (HttpStatusCode)500 },
            { ErrorCodeEnum.FFMPEG_OR_IMAGEMAGICK_FAILURE, (HttpStatusCode)500 },
        }
    }
);


```
