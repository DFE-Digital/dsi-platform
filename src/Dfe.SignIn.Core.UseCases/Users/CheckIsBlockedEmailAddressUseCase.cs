using System.Text.RegularExpressions;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Users;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Core.UseCases.Users;

/// <summary>
/// Options for <see cref="CheckIsBlockedEmailAddressUseCase"/>.
/// </summary>
public sealed class BlockedEmailAddressOptions : IOptions<BlockedEmailAddressOptions>
{
    /// <summary>
    /// Gets the list of blocked domain names.
    /// </summary>
    /// <remarks>
    ///   <example>
    ///     <para>Specify the list of blocked email domains:</para>
    ///     <code language="csharp"><![CDATA[
    ///       options.BlockedDomains = [
    ///           "example.com",
    ///           "example2.com",
    ///       ];
    ///     ]]></code>
    ///     <para>This would effectively block the use of the email address
    ///     "bob@example.com".</para>
    ///   </example>
    /// </remarks>
    public List<string> BlockedDomains { get; set; } = [];

    /// <summary>
    /// Gets the list of blocked user names.
    /// </summary>
    /// <remarks>
    ///   <example>
    ///     <para>Specify the list of blocked email user names:</para>
    ///     <code language="csharp"><![CDATA[
    ///       options.BlockedDomains = [
    ///           "admin",
    ///           "info",
    ///       ];
    ///     ]]></code>
    ///     <para>This would effectively block the use of the email address
    ///     "admin@example.com".</para>
    ///   </example>
    /// </remarks>
    public List<string> BlockedNames { get; set; } = [];

    /// <inheritdoc/>
    BlockedEmailAddressOptions IOptions<BlockedEmailAddressOptions>.Value => this;
}

/// <summary>
/// The use case for checking if an email address is blocked by the DfE Sign-in
/// email address policy.
/// </summary>
/// <param name="optionsAccessor">Provides access to use case options.</param>
public sealed partial class CheckIsBlockedEmailAddressUseCase(
    IOptionsMonitor<BlockedEmailAddressOptions> optionsAccessor
) : Interactor<CheckIsBlockedEmailAddressRequest, CheckIsBlockedEmailAddressResponse>
{
    /// <inheritdoc/>
    public override Task<CheckIsBlockedEmailAddressResponse> InvokeAsync(
        InteractionContext<CheckIsBlockedEmailAddressRequest> context,
        CancellationToken cancellationToken = default)
    {
        var options = optionsAccessor.CurrentValue;

        string[] parts = context.Request.EmailAddress.ToLower().Split('@');
        string name = TrimName().Replace(parts[0], "");
        string domain = parts[1];

        return Task.FromResult(new CheckIsBlockedEmailAddressResponse {
            IsBlocked = options.BlockedDomains.Contains(domain)
                || options.BlockedNames.Contains(name),
        });
    }

    [GeneratedRegex(@"[\d_.-]+$")]
    private static partial Regex TrimName();
}
