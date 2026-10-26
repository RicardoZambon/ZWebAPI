# ZWebAPI
ZWebAPI is a library that brings additional functionalities for web API projects.

## Supported frameworks

ZWebAPI ships assets for .NET 8, .NET 9 and .NET 10.

| Target framework | Notes |
| --- | --- |
| `net10.0` | LTS, supported until November 2028 |
| `net9.0` | STS, out of support since May 2026 |
| `net8.0` | LTS, out of support after November 2026 |

ZWebAPI depends on [ZDatabase](https://github.com/RicardoZambon/ZDatabase) and
[ZSecurity](https://github.com/RicardoZambon/ZSecurity), which ship the same three targets.
ZDatabase carries the EF Core dependency for each of them.

The `net9.0` and `net8.0` targets exist for compatibility with applications that have not yet
moved to .NET 10. Both of those runtimes are at or near end of support, so new projects should
target `net10.0`.

## Global exception handling

`AddGlobalExceptionHandling` turns any exception that leaves a controller into an RFC 7807 response,
so actions no longer need a `try/catch` each. It needs ASP.NET Core, which ZWebAPI references
through the `Microsoft.AspNetCore.App` shared framework.

```csharp
builder.Services.AddGlobalExceptionHandling(options =>
{
    // Application-specific mappings. They run after the defaults, so they can override them.
    options.Map<ValidationFailureException>((exception, _) =>
        ExceptionResponse.Content(StatusCodes.Status400BadRequest, exception.Message));
});

WebApplication app = builder.Build();
app.UseGlobalExceptionHandling(); // first in the pipeline
```

Default mappings:

| Exception | Response |
| --- | --- |
| `EntityNotFoundException<T>` | 404 problem |
| `EntityValidationFailureException<TKey>` | 400 validation problem, with the entity key in `entityKey` |
| `MissingUserPermissionException` | 403 problem |
| anything else | 500 problem |

A mapping matches the exact type first, then an open generic, then each base type.

Every problem carries `traceId`. When the response is a fault (500 and above, or a mapping that
calls `Capture()`), the exception goes to the `IExceptionReporter` registered by the application,
and the identifier it returns is written as `errorId`. Register the reporter as scoped. Without one,
nothing is reported.

The exception detail (message, inner message, stack trace, type) is written only when the host runs
in Development **and** `IsExceptionDetailIncludedInDevelopment` is `true`. It is off by default.

`ActionArgumentsBreadcrumbFilter` is registered with the handler. When an action throws, it adds the
action's arguments as a breadcrumb, so the reported error still says what the action received.

## Health checks

```csharp
builder.Services.AddDefaultHealthChecks();
// ...
app.MapDefaultHealthChecks(); // "/health" and "/health/ready"
```

- `/health` is the liveness probe. It runs no check, so a database outage does not restart the
  process.
- `/health/ready` is the readiness probe. It runs the checks tagged `ready`, which by default is the
  database check: `CanConnectAsync` on the registered `IDbContext`.
