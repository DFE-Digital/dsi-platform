namespace Dfe.SignIn.InternalApi.Configuration;

/// <summary>
/// 
/// </summary>
public sealed class EmailRestrictionsSettings
{
    /// <summary>
    /// Name of the key associated with the settings section held in configuration
    /// </summary>
    public const string SectionName = "EmailRestrictions";

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

    /// <summary>
    /// Gets the list of emails that are considered generic.
    /// </summary>
    /// <remarks>
    ///   <example>
    ///     <para>Specify the list of considered email user names:</para>
    ///     <code language="csharp"><![CDATA[
    ///       options.BlockedNames = [
    ///           "admin@admin.com",
    ///           "info@admin.com",
    ///       ];
    ///     ]]></code>
    ///     <para>This would effectively flag up the use of the email address
    ///     "info@admin.com".</para>
    ///   </example>
    /// </remarks>
    public List<string> GenericEmailStrings { get; set; } = [];

}
