using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SaudiPlus.Application.Interfaces;
using SaudiPlus.Infrastructure.Persistence;
namespace SaudiPlus.Infrastructure.Identity;
public sealed class IdentityService(UserManager<IdentityUser<Guid>> users, SaudiPlusDbContext context, IJwtTokenService tokens) : IIdentityService
{
    public async Task<bool> RegisterAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        if (!ValidInput(email, password)) return false;
        await using var transaction = await context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        var user = new IdentityUser<Guid> { UserName = email.Trim(), Email = email.Trim() };
        try
        {
            if (!(await users.CreateAsync(user, password)).Succeeded) return false;
            if (!(await users.AddToRoleAsync(user, IdentityRoles.Customer)).Succeeded) return false;
            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException) { return false; }
    }
    public async Task<AccessToken?> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await users.FindByEmailAsync(email.Trim());
        if (user is null || await users.IsLockedOutAsync(user)) return null;
        if (!await users.CheckPasswordAsync(user, password))
        {
            await users.AccessFailedAsync(user);
            return null;
        }
        if (!(await users.ResetAccessFailedCountAsync(user)).Succeeded) return null;
        cancellationToken.ThrowIfCancellationRequested();
        return tokens.Create(user.Id, user.Email!, await users.GetRolesAsync(user));
    }
    private static bool ValidInput(string email, string password) => !string.IsNullOrWhiteSpace(email) && email.Length <= 256 && new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email) && !string.IsNullOrEmpty(password) && password.Length <= 1024;
}
