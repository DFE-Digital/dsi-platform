using Dfe.SignIn.Core.Contracts.Features.Users.ChangeName;
using FluentValidation;

namespace Dfe.SignIn.Web.Profile.Configuration;

public static class ValidatorRegistrationExtensions
{
    /// <summary>
    /// Registers validators for the application.
    /// </summary>
    /// <param name="services">The services collection.</param>
    /// <returns>
    ///   <para>The <paramref name="services"/> instance for chained calls.</para>
    /// </returns>
    public static IServiceCollection AddDsiValidators(this IServiceCollection services)
    {
        services.AddSingleton<IValidator<ChangeNameRequest>, ChangeNameRequestValidator>();
        return services;
    }
}
