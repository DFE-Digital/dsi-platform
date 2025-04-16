using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dfe.SignIn.Core.ExternalModels.SelectOrganisation;

/// <summary>
/// A converter that converts a <see cref="SelectOrganisationCallbackSelection"/> to/from JSON.
/// </summary>
public sealed class SelectOrganisationCallbackSelectionJsonConverter : JsonConverter<SelectOrganisationCallbackSelection>
{
    /// <inheritdoc />
    public override SelectOrganisationCallbackSelection Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var propertyNamingPolicy = options.PropertyNamingPolicy!;

        using (JsonDocument doc = JsonDocument.ParseValue(ref reader)) {
            JsonElement root = doc.RootElement;

            var userIdProperty = doc.RootElement.GetProperty(
                propertyNamingPolicy.ConvertName(nameof(SelectOrganisationCallbackSelection.UserId))
            );
            var detailLevelProperty = doc.RootElement.GetProperty(
                propertyNamingPolicy.ConvertName(nameof(SelectOrganisationCallbackSelection.DetailLevel))
            );
            var selectionProperty = doc.RootElement.GetProperty(
                propertyNamingPolicy.ConvertName(nameof(SelectOrganisationCallbackSelection.Selection))
            );

            OrganisationDetailLevel detailLevel = (OrganisationDetailLevel)detailLevelProperty.Deserialize(typeof(OrganisationDetailLevel), options)!;
            var selectionType = SelectedOrganisation.ResolveType(detailLevel);

            return new SelectOrganisationCallbackSelection {
                Type = PayloadTypeConstants.Selection,
                DetailLevel = detailLevel,
                UserId = (Guid)userIdProperty.Deserialize(typeof(Guid), options)!,
                Selection = (SelectedOrganisation)selectionProperty.Deserialize(selectionType, options)!,
            };
        }
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, SelectOrganisationCallbackSelection value, JsonSerializerOptions options)
    {
        var propertyNamingPolicy = options.PropertyNamingPolicy!;

        writer.WriteStartObject();
        writer.WriteString(nameof(SelectOrganisationCallbackSelection.Type), PayloadTypeConstants.Selection);

        var userIdConverter = options.GetConverter(typeof(Guid)) as JsonConverter<Guid>;
        writer.WritePropertyName(propertyNamingPolicy.ConvertName(nameof(SelectOrganisationCallbackSelection.UserId)));
        userIdConverter!.Write(writer, value.UserId, options);

        var detailLevelConverter = options.GetConverter(typeof(OrganisationDetailLevel)) as JsonConverter<OrganisationDetailLevel>;
        writer.WritePropertyName(propertyNamingPolicy.ConvertName(nameof(SelectOrganisationCallbackSelection.DetailLevel)));
        detailLevelConverter!.Write(writer, value.DetailLevel, options);

        var selectionConverter = options.GetConverter(typeof(SelectedOrganisation)) as JsonConverter<SelectedOrganisation>;
        writer.WritePropertyName(propertyNamingPolicy.ConvertName(nameof(SelectOrganisationCallbackSelection.Selection)));
        JsonSerializer.Serialize(writer, value.Selection, value.Selection.GetType(), options);

        writer.WriteEndObject();
    }
}
