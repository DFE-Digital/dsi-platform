using System.ComponentModel.DataAnnotations;
using System.Runtime.ExceptionServices;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Core.Framework;

/// <summary>
/// Decorates interactor with request and response model validation using validation
/// techniques from the <see cref="System.ComponentModel.DataAnnotations"/>
/// namespace.
/// </summary>
/// <typeparam name="TRequest">The type of request.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
/// <param name="options">Interactor model validation options.</param>
/// <param name="inner">The inner interactor that is being decorated.</param>
/// <param name="services">The service provider.</param>
public sealed class InteractorModelValidator<TRequest, TResponse>(
    IOptions<InteractorModelValidationOptions> options,
    IInteractor<TRequest, TResponse> inner,
    IServiceProvider services
) : IInteractor<TRequest, TResponse>
    where TRequest : class
    where TResponse : class
{
    /// <inheritdoc/>
    public async Task<TResponse> InvokeAsync(TRequest request, CancellationToken cancellationToken = default)
    {
        if (options.Value.ValidateRequestModels) {
            this.Validate(request!);
        }

        TResponse? response = null;

        try {
            response = await inner.InvokeAsync(request, cancellationToken);
        }
        catch (Exception ex) {
            if (ex is not InteractionException and not OperationCanceledException) {
                // Inner validation exceptions fall into this category since they
                // are unexpected from the context of this particular interaction.
                ex = new UnexpectedException("An unexpected exception occurred whilst processing interaction.", ex);
            }
            ExceptionDispatchInfo.Capture(ex).Throw();
            throw; // Keep compiler happy.
        }

        if (options.Value.ValidateResponseModels) {
            this.Validate(response);
        }
        return response;
    }

    private void Validate(object instance)
    {
        var context = new ValidationContext(instance);
        context.InitializeServiceProvider(services.GetService);
        Validator.ValidateObject(instance, context, validateAllProperties: true);
    }
}
