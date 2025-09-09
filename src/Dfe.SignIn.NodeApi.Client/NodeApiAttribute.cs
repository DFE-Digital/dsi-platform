namespace Dfe.SignIn.NodeApi.Client;

/// <summary>
/// An attribute that associates an <see cref="Base.Framework.IInteractor{TRequest}"/>
/// implementation with the corresponding Node.js API.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class NodeApiAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NodeApiAttribute"/> class.
    /// </summary>
    /// <param name="name">The name of the associated Node.js API.</param>
    public NodeApiAttribute(NodeApiName name)
    {
        this.Name = name;
    }

    /// <summary>
    /// Gets the name of the associated Node.js API.
    /// </summary>
    public NodeApiName Name { get; init; }
}
