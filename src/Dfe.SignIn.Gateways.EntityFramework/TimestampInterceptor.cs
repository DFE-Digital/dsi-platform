using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Dfe.SignIn.Gateways.EntityFramework;

/// <summary>
/// Intercepts EF Core SaveChanges operations to automatically set CreatedAt and UpdatedAt timestamps.
/// </summary>
/// <param name="timeProvider">Provides the current time.</param>
internal class TimestampInterceptor(TimeProvider timeProvider) : SaveChangesInterceptor
{
    /// <summary>
    /// Called synchronously before SaveChanges is executed.
    /// Updates timestamps for entities tracked in the DbContext.
    /// </summary>
    /// <param name="eventData">Provides context information about the SaveChanges operation.</param>
    /// <param name="result">The current interception result.</param>
    /// <returns>The interception result, typically passed to the base implementation.</returns>
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        this.ApplyTimestamps(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <summary>
    /// Called asynchronously before SaveChangesAsync is executed.
    /// Updates timestamps for entities tracked in the DbContext.
    /// </summary>
    /// <param name="eventData">Provides context information about the SaveChanges operation.</param>
    /// <param name="result">The current interception result.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The interception result, typically passed to the base implementation.</returns>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        this.ApplyTimestamps(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// Iterates over all tracked entities and sets CreatedAt and UpdatedAt timestamps as needed.
    /// </summary>
    /// <param name="context">The DbContext containing tracked entities.</param>
    private void ApplyTimestamps(DbContext? context)
    {
        if (context == null) {
            return;
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;

        foreach (var entry in context.ChangeTracker.Entries()) {
            if (entry.State == EntityState.Added) {
                SetIfExists(entry, "CreatedAt", now);
            }

            if (entry.State is EntityState.Added or EntityState.Modified) {
                SetIfExists(entry, "UpdatedAt", now);
            }
        }
    }

    /// <summary>
    /// Sets the value of a property on an entity if it exists.
    /// </summary>
    /// <param name="entry">The entity entry being updated.</param>
    /// <param name="propertyName">The name of the property to set.</param>
    /// <param name="value">The value to assign.</param>
    private static void SetIfExists(EntityEntry entry, string propertyName, object value)
    {
        var prop = entry.Metadata.FindProperty(propertyName);
        if (prop is null) {
            return;
        }

        entry.Property(propertyName).CurrentValue = value;
    }
}
