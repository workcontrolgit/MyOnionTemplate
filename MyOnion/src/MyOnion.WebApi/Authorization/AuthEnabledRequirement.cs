using Microsoft.FeatureManagement;

namespace MyOnion.WebApi.Authorization;

public sealed class AuthEnabledRequirement : IAuthorizationRequirement
{
}

public sealed class AuthEnabledRequirementHandler : AuthorizationHandler<AuthEnabledRequirement>
{
    private readonly IFeatureManagerSnapshot _featureManager;

    public AuthEnabledRequirementHandler(IFeatureManagerSnapshot featureManager)
    {
        _featureManager = featureManager;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AuthEnabledRequirement requirement)
    {
        var enabled = await _featureManager.IsEnabledAsync("AuthEnabled").ConfigureAwait(false);
        if (!enabled)
        {
            context.Succeed(requirement);
            return;
        }

        if (context.User?.Identity?.IsAuthenticated == true)
        {
            context.Succeed(requirement);
        }
    }
}
