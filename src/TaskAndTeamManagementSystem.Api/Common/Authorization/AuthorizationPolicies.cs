using Microsoft.AspNetCore.Authorization;

namespace TaskAndTeamManagementSystem.Api.Common.Authorization;

public static class AuthorizationPolicies
{
    public static void ConfigurePolicies(AuthorizationOptions options)
    {
        options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
        options.AddPolicy("ManagerOrAdmin", policy => policy.RequireRole("Manager", "Admin"));
        options.AddPolicy("EmployeeOrAbove", policy => policy.RequireRole("Employee", "Manager", "Admin"));
    }
}

