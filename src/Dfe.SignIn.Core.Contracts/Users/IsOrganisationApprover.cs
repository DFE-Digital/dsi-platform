using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Users;

/// <summary>
/// Request model for checking if a user is an approver for any of their associated organisations.
/// </summary>
[AssociatedResponse(typeof(IsOrganisationApproverResponse))]
public sealed record IsOrganisationApproverRequest(Guid UserId);

/// <summary>
/// Response model for request <see cref="IsOrganisationApproverRequest"/>.
/// </summary>
public sealed record IsOrganisationApproverResponse(bool IsApprover);
