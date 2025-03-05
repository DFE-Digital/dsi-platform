namespace Dfe.SignIn.Core.Models.Applications;

public sealed record ServiceApiSecretModel()
{
    /// <summary>
    /// Unique Id of the service
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// ApiSecret for the services
    /// </summary>
    public string? ApiSecret { get; init; }
}
