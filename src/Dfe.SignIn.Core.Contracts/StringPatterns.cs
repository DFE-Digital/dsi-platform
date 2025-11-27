using System.Text.RegularExpressions;

namespace Dfe.SignIn.Core.Contracts;

/// <summary>
/// Common regular expression patterns for validating strings.
/// </summary>
public static partial class StringPatterns
{
    /// <summary>
    /// Regular expression pattern which can be used to verify the name of a person.
    /// Suitable for first name, last name, or full name.
    /// </summary>
    public const string PersonNamePattern
        = @"^[\p{L}\p{N}_][\p{L}\p{N}_'-]*( [\p{L}\p{N}][\p{L}\p{N} _'-]*)*$";

    [GeneratedRegex(PersonNamePattern)]
    public static partial Regex PersonNameRegex();

    /// <summary>
    /// Regular expression pattern which can be used to verify an email address.
    /// </summary>
    /// <remarks>
    ///   <para>Whilst this expression does not accurately represent the RFC standard;
    ///   it should catch obvious errors in user input.</para>
    /// </remarks>
    public const string EmailAddressPattern
        = @"^[\p{L}\p{N}_']([\p{L}\p{N}_'+.-]*[\p{L}\p{N}_'])?@[\p{L}\p{N}_-]+(\.[\p{L}\p{N}_-]*[\p{L}\p{N}])+$";

    [GeneratedRegex(EmailAddressPattern)]
    public static partial Regex EmailAddressRegex();

    /// <summary>
    /// Regular expression pattern which can be used to verify the job title of a person.
    /// </summary>
    public const string JobTitlePattern
        = @"^([\p{L}\p{N}()]+( [\p{L}\p{N}()]+)*)?$";

    [GeneratedRegex(JobTitlePattern)]
    public static partial Regex JobTitleRegex();
}
