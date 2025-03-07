# SbuTils
Nuget package: https://www.nuget.org/packages/SbuTils/

## Aspnet core catch-all exceptions middleware

Use this middleware in conjonction with domain exception and error codes to streamline the error handling process in a json rest API.

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
            ...

            // 500s
            { ErrorCodeEnum.ITEM_FILE_NOT_FOUND, 500 },
            ...
        }
    }
);
```

## Result container

Sometimes we don't want a process to throw, we want to give the Exception handling responsibility to the caller. 
`Result` gives the flexibility for the service returning it to either succeed or fail without throwing. 

1. If you haven't already, create an "error code enum" that will contain your app's error codes: 
 
```csharp
public enum ErrorCodeEnum
{
    ITEM_NOT_FOUND,
    GROUP_ITEM_NOT_FOUND,
    ITEM_FILE_NOT_FOUND
    /* ... */
}

```

2. Whenever a service you create should not throw but could fail, make it return `Error<ErrorCodeEnum>` or `Error<ExpectedReturnedType, ErrorCodeEnum>` and give the "throw exception" responsibility to the caller.
