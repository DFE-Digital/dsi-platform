using FluentValidation;

namespace Dfe.SignIn.PublicApi.Endpoints.Users.GetServiceUsers;

/// <summary>
/// Provides validation logic for the GetServiceUsersQuery, ensuring that query parameters such as pagination, status,
/// and date ranges meet required constraints before processing.
/// </summary>
/// <remarks>This validator enforces that page and page size values are positive, status values are limited to
/// valid options, and date ranges are within allowed limits and not set in the future. Use this class to validate
/// incoming GetServiceUsersQuery instances prior to executing queries against the service user data source.</remarks>
public class GetServiceUsersQueryValidator : AbstractValidator<GetServiceUsersQuery>
{
    private const int MaxDateRangeDays = 90;

    /// <inheritdoc />
    public GetServiceUsersQueryValidator()
    {
        this.RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("page must be greater than 0.");
        this.RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("pageSize must be greater than 0.");

        this.When(x => x.Status.HasValue, () => {
            this.RuleFor(x => x.Status)
                .Must(s => s is 0 or 1)
                .WithMessage("Status is not valid. Should be either 0 or 1.");
        });

        this.When(x => x.From.HasValue && x.To.HasValue, () => {
            this.RuleFor(x => x)
                .Must(x => x.From!.Value <= x.To!.Value)
                .WithMessage("From date greater than to date.")
                .Must(x => x.From!.Value <= DateTimeOffset.UtcNow && x.To!.Value <= DateTimeOffset.UtcNow)
                .WithMessage("Date range should not be in the future.")
                .Must(x => (x.To!.Value - x.From!.Value).TotalDays <= MaxDateRangeDays)
                .WithMessage($"Only {MaxDateRangeDays} days are allowed between dates.");
        });

        this.When(x => x.From.HasValue && !x.To.HasValue, () => {
            this.RuleFor(x => x.From)
                .Must(f => f!.Value <= DateTimeOffset.UtcNow)
                .WithMessage("Date range should not be in the future.");
        });

        this.When(x => x.To.HasValue && !x.From.HasValue, () => {
            this.RuleFor(x => x.To)
                .Must(t => t!.Value <= DateTimeOffset.UtcNow)
                .WithMessage("Date range should not be in the future.");
        });
    }
}
