namespace Dfe.SignIn.Base.Framework;

/// <summary>
/// Request types can be annotated with this attribute to indicate that the request
/// can lead to exceptions of the given type.
/// </summary>
/// <remarks>
///   <para>This attribute is useful for automatically generated documentation.</para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class ThrowsAttribute(Type exceptionType) : Attribute
{
    /// <summary>
    /// Gets the type of exception that can be thrown.
    /// </summary>
    public Type ExceptionType => exceptionType;
}

/// <summary>
/// Request types can be annotated with this attribute to indicate the response type
/// that is associated with the request type.
/// </summary>
/// <remarks>
///   <para>This attribute is useful for automatically generated documentation.</para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class AssociatedResponseAttribute(Type responseType) : Attribute
{
    /// <summary>
    /// Gets the type of response that is associated with the request.
    /// </summary>
    public Type ResponseType => responseType;
}
