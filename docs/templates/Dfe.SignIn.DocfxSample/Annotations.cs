namespace Dfe.SignIn.Base.Framework;

// Polyfill from 'Dfe.SignIn.Base.Framework' to avoid need for direct dependency.

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class ThrowsAttribute(Type exceptionType) : Attribute
{
    public Type ExceptionType => exceptionType;
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class AssociatedResponseAttribute(Type responseType) : Attribute
{
    public Type ResponseType => responseType;
}
