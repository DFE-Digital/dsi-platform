namespace Dfe.SignIn.Core.Framework;

/// <summary>
/// Used to annotate custom exception properties such that they are included when
/// an exception is serialized by <see cref="IExceptionJsonSerializer"/>.
/// </summary>
/// <remarks>
///   <para>In order for this to work the annotated property must:</para>
///   <list type="bullet">
///     <item>have a public or non-public getter.</item>
///     <item>have a public or non-public setter.</item>
///   </list>
///   <example>
///     <para>An example of a custom exception type with a persisted property:</para>
///     <code language="csharp"><![CDATA[
///       public sealed class ApplicationDoesNotExistException : InteractionException
///       {
///           public ApplicationDoesNotExistException() { }
///
///           public ApplicationDoesNotExistException(string message)
///               : base(message) { }
///
///           public ApplicationDoesNotExistException(string message, Exception innerException)
///               : base(message, innerException) { }
///
///           public ApplicationDoesNotExistException(Guid applicationId, string clientId)
///           {
///               this.ApplicationId = applicationId;
///               this.ClientId = clientId;
///           }
///
///           [Persist]
///           public Guid? ApplicationId { get; private set; }
///
///           [Persist]
///           public string ClientId { get; private set; }
///       }
///     ]]></code>
///   </example>
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class PersistAttribute : Attribute
{
}
