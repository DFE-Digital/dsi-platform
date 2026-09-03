using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Core.UseCases.Users;

/// <summary>
/// Options for Blocked Email address configuration/>.
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
    ///       options.BlockedNames = [
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
