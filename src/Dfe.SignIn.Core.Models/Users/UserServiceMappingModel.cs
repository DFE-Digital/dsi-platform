public sealed record UserServiceMappingModel()
{
    public required Guid UserId { get; init; } // Can this ever be missing?

    public required Guid InvitationId { get; init; } // Can this ever be missing?

    public required Guid ApplicationId { get; init; }

    public required Guid OrganisationId { get; init; } // Can this ever be missing?

    // Roles?

    // Identifiers?

    public required DateTime AccessGranted { get; init; }
}
