using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.InternalApi.Features.Users.AutoLinkEntraToDsi.Models;
using Dfe.SignIn.InternalApi.Features.Users.ChangePassword;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.InternalApi.Features.Users.AutoLinkEntraToDsi.Services;

/// <summary>
/// 
/// </summary>
public interface IUserCreator
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="userDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<UserEntity> CreateAsync(User userDto, CancellationToken cancellationToken);
}

/// <summary>
/// 
/// </summary>
/// <param name="directoriesDbContext"></param>
/// <param name="config"></param>
/// <param name="passwordHasher"></param>
public sealed class UserCreator(DbDirectoriesContext directoriesDbContext,
    IConfiguration config,
    IPasswordHasher passwordHasher) : IUserCreator
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="userDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<UserEntity> CreateAsync(User userDto, CancellationToken cancellationToken)
    {
        var exstingUser = await directoriesDbContext.Users
            .FirstOrDefaultAsync(x => x.Email == userDto.Username, cancellationToken);

        if (exstingUser is not null) {
            return exstingUser;
        }

        var salt = passwordHasher.GenerateSalt();

        var newUser = new UserEntity {
            Sub = Guid.NewGuid(),
            IsInternalUser = false,
            FirstName = userDto.FirstName,
            Email = userDto.Username,
            LastName = userDto.LastName,
            Salt = salt,
            // we're an entra account.
            // We have no responsbility when it comes to password
            Password = "none",
            Status = 1,
            PasswordResetRequired = false,
            IsEntra = true,
            EntraOid = userDto.EntraOid,
            EntraLinked = DateTime.UtcNow
        };

        await directoriesDbContext.Users.AddAsync(newUser, cancellationToken);

        //TOOD: Move to Ioptions and provide v4 as default!
        var activePasswordPolicyCode = config.GetValue<string>("POLICY_CODE") ?? PasswordHasher.LatestPolicyCode;

        directoriesDbContext.UserPasswordPolicies.Add(new UserPasswordPolicyEntity {
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Id = Guid.NewGuid(),
            Uid = newUser.Sub,
            PasswordHistoryLimit = 3,
            PolicyCode = activePasswordPolicyCode
        });

        await directoriesDbContext.SaveChangesAsync(cancellationToken);

        return newUser;
    }
}
