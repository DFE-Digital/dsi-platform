using System.Collections.Concurrent;
using Dfe.SignIn.Core.Interfaces.Notifications;

namespace Dfe.SignIn.TestHelpers.Integration.Mocks;

/// <summary>
/// A fake implementation of <see cref="INotificationService"/> that tracks sent email requests for test assertions.
/// </summary>
public sealed class FakeEmailNotificationService(FakeEmailRequestTracker? tracker = null)
    : INotificationService
{
    private readonly FakeEmailRequestTracker? tracker = tracker;

    public Task SendAsync(
        string recipientEmailAddress,
        string templateId,
        IReadOnlyDictionary<string, dynamic> personalisation)
    {
        this.tracker?.Track(new TrackedEmailRequest(recipientEmailAddress, templateId, personalisation));
        return Task.CompletedTask;
    }
}

/// <summary>
/// Represents an email dispatch request recorded during testing.
/// </summary>
public sealed record TrackedEmailRequest(
    string RecipientEmailAddress,
    string TemplateId,
    IReadOnlyDictionary<string, dynamic> Personalisation);

/// <summary>
/// Holds tracked email requests emitted during test execution.
/// </summary>
public sealed class FakeEmailRequestTracker
{
    private readonly ConcurrentBag<TrackedEmailRequest> requests = [];

    public IReadOnlyCollection<TrackedEmailRequest> Requests => [.. this.requests];

    public void Track(TrackedEmailRequest request)
    {
        this.requests.Add(request);
    }

    public void Clear()
    {
        this.requests.Clear();
    }
}
