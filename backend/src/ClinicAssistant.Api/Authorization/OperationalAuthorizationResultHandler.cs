using ClinicAssistant.Application.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace ClinicAssistant.Api.Authorization;

public sealed class OperationalAuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

    public Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Challenged || authorizeResult.Forbidden)
            OperationalTelemetry.AuthorizationDenied.Add(1);

        return _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }
}
