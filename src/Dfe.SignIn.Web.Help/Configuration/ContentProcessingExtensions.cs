using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Web.Help.Content;
using Dfe.SignIn.Web.Help.Content.Processing;
using Dfe.SignIn.WebFramework.Configuration;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Web.Help.Configuration;

/// <summary>
/// Extension methods for setting up content processing services.
/// </summary>
[ExcludeFromCodeCoverage]
public static class ContentProcessingExtensions
{
    /// <summary>
    /// Setup content processing services.
    /// </summary>
    /// <param name="services">The collection to add services to.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="services"/> is null.</para>
    /// </exception>
    public static void SetupContentProcessing(this IServiceCollection services)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));

        services.AddSingleton<ITopicMarkdownProcessor, MarkdigTopicMarkdownProcessor>();
        services.AddSingleton<ITopicFileReader, TopicFileReader>();
        services.AddSingleton<ITopicFilePipeline, TopicFilePipeline>();
        services.AddSingleton<ITopicIndexAccessor, TopicMemoryCacheAccessor>();

        services.AddSingleton<ITopicPreProcessor, VariableSubstitutionTopicPreProcessor>(provider => {
            var platformOptionsAccessor = provider.GetRequiredService<IOptions<PlatformOptions>>();
            var platformOptions = platformOptionsAccessor.Value;
            return new VariableSubstitutionTopicPreProcessor(new VariableSubstitutionOptions {
                Variables = {
                    ["HELP_URL"] = platformOptions.HelpUrl.ToString(),
                    ["MANAGE_URL"] = platformOptions.ManageUrl.ToString(),
                    ["PROFILE_URL"] = platformOptions.ProfileUrl.ToString(),
                    ["SERVICES_URL"] = platformOptions.ServicesUrl.ToString(),
                    ["SUPPORT_URL"] = platformOptions.SupportUrl.ToString(),
                },
            });
        });
    }
}
