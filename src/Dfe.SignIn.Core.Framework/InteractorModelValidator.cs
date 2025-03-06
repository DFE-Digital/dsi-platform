using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Core.Framework;

/// <summary>
/// Decorates interactor with request and response model validation using validation
/// techniques from the <see cref="System.ComponentModel.DataAnnotations"/>
/// namespace.
/// </summary>
/// <typeparam name="TRequest">The type of request.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
public sealed class InteractorModelValidator<TRequest, TResponse>(
    IOptions<InteractorModelValidationOptions> options,
    IInteractor<TRequest, TResponse> inner,
    IServiceProvider services
) : IInteractor<TRequest, TResponse>
{
    /// <inheritdoc/>
    public async Task<TResponse> InvokeAsync(TRequest request)
    {
        if (options.Value.ValidateRequestModels) {
            this.Validate(request!);
        }
        var response = await inner.InvokeAsync(request);
        if (options.Value.ValidateResponseModels) {
            this.Validate(response!);
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
