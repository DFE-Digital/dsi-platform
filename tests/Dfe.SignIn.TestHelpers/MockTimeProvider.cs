namespace Dfe.SignIn.TestHelpers;

/// <summary>
/// An implementation of TimeProvider for mocking purposes.
/// </summary>
public sealed class MockTimeProvider : TimeProvider
{
    private DateTimeOffset utcNow;

    /// <summary>
    /// Construct an instance of MockTimeProvider.
    /// </summary>
    /// <param name="startTime">The moment in time which to start from.</param>
    public MockTimeProvider(DateTimeOffset startTime)
    {
        this.utcNow = startTime;
    }

    /// <summary>
    /// Gets the current DateTimeOffset associated with the instance.
    /// </summary>
    /// <returns></returns>
    public override DateTimeOffset GetUtcNow() => this.utcNow;

    /// <summary>
    /// Advance the DateTimeOffset.
    /// </summary>
    /// <param name="timeSpan">Adjust the DateTimeOffset by the provided TimeSpan.</param>
    public void Advance(TimeSpan timeSpan)
    {
        this.utcNow = this.utcNow.Add(timeSpan);
    }
}
