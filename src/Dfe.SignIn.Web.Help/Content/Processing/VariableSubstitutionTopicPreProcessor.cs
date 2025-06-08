using System.Text.RegularExpressions;

namespace Dfe.SignIn.Web.Help.Content.Processing;

/// <summary>
/// Options for the <see cref="VariableSubstitutionTopicPreProcessor"/> service.
/// </summary>
public sealed class VariableSubstitutionOptions
{
    /// <summary>
    /// Gets or sets the variable key/value pairs.
    /// </summary>
    public Dictionary<string, string> Variables { get; set; } = [];
}

/// <summary>
/// A topic pre-processor which finds references to DfE Sign-in URLs and corrects them
/// for the current hosting environment.
/// </summary>
public partial class VariableSubstitutionTopicPreProcessor(
    VariableSubstitutionOptions options
) : ITopicPreProcessor
{
    [GeneratedRegex("\\${{([^}]+)}}")]
    private static partial Regex VariablePlaceholderPattern();

    /// <inheritdoc/>
    public Task<string> ProcessAsync(string topicPath, string markdown, CancellationToken cancellationToken = default)
    {
        markdown = VariablePlaceholderPattern().Replace(markdown, matches => {
            string variableName = matches.Groups[1].Value.Trim();
            options.Variables.TryGetValue(variableName, out string? variableValue);
            return variableValue ?? throw new KeyNotFoundException($"Cannot resolve variable '{matches.Value}' in topic '{topicPath}'.");
        });
        return Task.FromResult(markdown);
    }
}
