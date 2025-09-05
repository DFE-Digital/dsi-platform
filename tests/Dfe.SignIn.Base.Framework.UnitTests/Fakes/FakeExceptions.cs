namespace Dfe.SignIn.Base.Framework.UnitTests.Fakes;

public sealed class FakeInteractionException : InteractionException
{
    public FakeInteractionException() { }

    public FakeInteractionException(string message)
        : base(message) { }

    public FakeInteractionException(string message, Exception innerException)
        : base(message, innerException) { }
}

public sealed class FakeInteractionExceptionWithPeristedProperties : InteractionException
{
    public FakeInteractionExceptionWithPeristedProperties() { }

    public FakeInteractionExceptionWithPeristedProperties(string message)
        : base(message) { }

    public FakeInteractionExceptionWithPeristedProperties(string message, Exception innerException)
        : base(message, innerException) { }

    public FakeInteractionExceptionWithPeristedProperties(string message, string customProperty)
        : base(message)
    {
        this.CustomProperty = customProperty;
    }

    [Persist]
    public string CustomProperty { get; private set; } = "";

    [Persist]
    public string ComputedProperty => $"Computed: {this.CustomProperty}";
}

public sealed class FakeInteractionExceptionWithMissingConstructor : InteractionException
{
}
