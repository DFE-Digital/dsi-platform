using System.Collections.ObjectModel;

namespace Dfe.SignIn.Core.Contracts.Audit;

public static class AuditType
{
    public const string ChangeEmail = "change-email";
    public const string ChangeJobTitle = "change-job-title";
    public const string ChangeName = "change-name";
    public const string ChangePassword = "change-password";
}

public static class AuditSubType
{
}

public sealed record WriteToAuditRequest
{
    public required string Type { get; init; }

    public string? SubType { get; init; } = null;

    public required string Message { get; init; }

    public Guid? UserId { get; init; }

    public Guid? OrganisationId { get; init; }

    public bool Success { get; init; } = true;

    public IReadOnlyDictionary<string, string> Meta { get; init; } = ReadOnlyDictionary<string, string>.Empty;
}

public sealed record WriteToAuditResponse
{
}
