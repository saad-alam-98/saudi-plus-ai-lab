using Microsoft.AspNetCore.Identity;

namespace SaudiPlus.Infrastructure.Identity;

public static class IdentityRoles
{
    public const string Customer = "Customer";
    public const string Merchant = "Merchant";
    public const string Administrator = "Administrator";

    public static readonly IReadOnlyList<string> All = [Customer, Merchant, Administrator];
}

public static class RoleInitializer
{
    public static async Task InitializeAsync(
        RoleManager<IdentityRole<Guid>> roleManager,
        CancellationToken cancellationToken = default)
    {
        foreach (var name in IdentityRoles.All)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (await roleManager.RoleExistsAsync(name))
                continue;

            var result = await roleManager.CreateAsync(new IdentityRole<Guid>(name));
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    $"Could not initialize role '{name}': " +
                    string.Join("; ", result.Errors.Select(error => error.Description)));
        }
    }
}
