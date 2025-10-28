using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;

namespace Dfe.SignIn.NodeApi.Client.Users;

/// <summary>
/// A fake "write to audit" placeholder implementation (to be replaced).
/// </summary>
[ApiRequester]
[NodeApi(NodeApiName.Directories)]
public sealed class WriteToAuditNodeRequester : Interactor<WriteToAuditRequest, WriteToAuditResponse>
{
    /// <inheritdoc/>
    public override async Task<WriteToAuditResponse> InvokeAsync(
        InteractionContext<WriteToAuditRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        Console.WriteLine($"audit: {context.Request.Message}");

        return new WriteToAuditResponse();
    }
}
