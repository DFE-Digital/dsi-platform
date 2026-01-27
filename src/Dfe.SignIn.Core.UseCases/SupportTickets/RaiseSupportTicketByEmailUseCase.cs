using System.Text.RegularExpressions;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Notifications;
using Dfe.SignIn.Core.Contracts.SupportTickets;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Core.UseCases.SupportTickets;

/// <summary>
/// Options for <see cref="RaiseSupportTicketByEmailUseCase"/>.
/// </summary>
public sealed class RaiseSupportTicketByEmailOptions : IOptions<RaiseSupportTicketByEmailOptions>
{
    /// <summary>
    /// ID of the support request email template.
    /// </summary>
    public required string EmailTemplateId { get; set; }

    /// <summary>
    /// URL of the "Contact us" page to be included in support emails.
    /// </summary>
    public required Uri ContactUrl { get; set; }

    /// <summary>
    /// Email address of the support team.
    /// </summary>
    public required string SupportEmailAddress { get; set; }

    /// <inheritdoc/>
    RaiseSupportTicketByEmailOptions IOptions<RaiseSupportTicketByEmailOptions>.Value => this;
}

/// <summary>
/// Use case for raising a support ticket by an email notification.
/// </summary>
public sealed partial class RaiseSupportTicketByEmailUseCase(
    IOptions<RaiseSupportTicketByEmailOptions> optionsAccessor,
    IInteractionDispatcher interaction
) : Interactor<RaiseSupportTicketRequest, RaiseSupportTicketResponse>
{
    /// <inheritdoc/>
    public override async Task<RaiseSupportTicketResponse> InvokeAsync(
        InteractionContext<RaiseSupportTicketRequest> context,
        CancellationToken cancellationToken = default)
    {
        await this.ValidateSubjectCodeAsync(context);

        context.ThrowIfHasValidationErrors();

        var options = optionsAccessor.Value;

        if (string.IsNullOrWhiteSpace(options.SupportEmailAddress)) {
            throw new InvalidOperationException("Missing required configuration");
        }

        string supportEmailMessage = IsValidTraceId(context.Request.ExceptionTraceId ?? string.Empty)
            ? $"{context.Request.Message}{Environment.NewLine}(TraceId: {context.Request.ExceptionTraceId})"
            : context.Request.Message;

        await interaction.DispatchAsync(
            new SendEmailNotificationRequest {
                RecipientEmailAddress = options.SupportEmailAddress,
                TemplateId = options.EmailTemplateId,
                Personalisation = new() {
                    ["name"] = context.Request.FullName,
                    ["email"] = context.Request.EmailAddress,
                    ["orgName"] = context.Request.OrganisationName,
                    ["urn"] = context.Request.OrganisationUrn ?? "",
                    ["service"] = context.Request.ApplicationName ?? "",
                    ["type"] = context.Request.SubjectCode,
                    ["message"] = supportEmailMessage,
                    ["showAdditionalInfoHeader"] = !string.IsNullOrWhiteSpace(context.Request.CustomSummary),
                    ["typeAdditionalInfo"] = context.Request.CustomSummary ?? "",
                    ["helpUrl"] = options.ContactUrl,
                }
            }
        );

        return new RaiseSupportTicketResponse();
    }

    [GeneratedRegex(@"^[0-9a-f]{2}-[0-9a-f]{32}-[0-9a-f]{16}-[0-9a-f]{2}$")]
    private static partial Regex TraceIdPattern();

    private static bool IsValidTraceId(string traceId)
    {
        return TraceIdPattern().IsMatch(traceId);
    }

    private async Task ValidateSubjectCodeAsync(InteractionContext<RaiseSupportTicketRequest> context)
    {
        string propertyName = nameof(RaiseSupportTicketRequest.SubjectCode);

        // Don't proceed with validation if a validation error has already occurred
        // for the subject code request property.
        if (context.ValidationResults.Any(result => result.MemberNames.Contains(propertyName))) {
            return;
        }

        var subjectOptionsResponse = await interaction.DispatchAsync(
            new GetSubjectOptionsForSupportTicketRequest()
        ).To<GetSubjectOptionsForSupportTicketResponse>();

        if (!subjectOptionsResponse.SubjectOptions.Any(option => option.Code == context.Request.SubjectCode)) {
            context.AddValidationError("Invalid selection", propertyName);
        }
    }
}
