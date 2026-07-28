using System.Text;
using System.Text.Json;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Interfaces.Audit;

namespace Dfe.SignIn.Gateways.ServiceBus.Audit;

/// <summary>
/// Static serializer - moved to allow for shared code. Should be
/// moved back into the AuditWriter when WriteToAuditWithService class
/// is removed.
/// </summary>
internal static class AuditSerializer
{
    /// <summary>
    /// Serializes the audit context and request into a structured JSON payload.
    /// </summary>
    /// <param name="auditContext">The context containing trace and source metadata.</param>
    /// <param name="request">The audit request containing event details.</param>
    /// <returns>
    ///   <para>A JSON encoded string representing the audit event.</para>
    /// </returns>
    internal static string SerializeMessageBody(AuditContext auditContext, WriteToAuditRequest request)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        writer.WriteStartObject();

        if (!string.IsNullOrWhiteSpace(auditContext.SourceApplication)) {
            writer.WriteString("application", auditContext.SourceApplication);
        }
        if (!string.IsNullOrWhiteSpace(auditContext.SourceIpAddress)) {
            writer.WriteString("requestIp", auditContext.SourceIpAddress);
        }

        Guid? userId = request.UserId ?? auditContext.SourceUserId;
        if (userId is not null) {
            writer.WriteString("userId", userId.ToString());
        }

        if (request.OrganisationId is not null) {
            writer.WriteString("organisationid", request.OrganisationId.ToString());
        }

        writer.WriteString("type", request.EventCategory);
        writer.WriteString("subType", request.EventName);
        writer.WriteString("message", request.Message);
        writer.WriteString("env", auditContext.EnvironmentName);

        // The following property was originally output by the node implementation.
        // It seems to be redundant since all audit entries have this set.
        // writer.WriteString("level", "audit")

        writer.WriteStartObject("meta");
        writer.WriteString("req", auditContext.TraceId);
        writer.WriteBoolean("success", !request.WasFailure);
        foreach (var customProperty in request.CustomProperties) {
            writer.WritePropertyName(customProperty.Key);
            JsonSerializer.Serialize(writer, customProperty.Value);
        }
        writer.WriteEndObject();

        writer.WriteEndObject();
        writer.Flush();

        return Encoding.UTF8.GetString(stream.ToArray());
    }
}
