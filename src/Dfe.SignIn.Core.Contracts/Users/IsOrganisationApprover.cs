
namespace Dfe.SignIn.Core.Contracts.Users;

/// <summary>
/// Response model for determing if a user is an organisation approver.
/// </summary>
public sealed record IsOrganisationApproverResponse(bool IsApprover);
