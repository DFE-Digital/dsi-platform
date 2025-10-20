using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Base.Framework;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Dfe.SignIn.WebFramework.Mvc.ModelBinding;

/// <summary>
/// Decorates an existing <see cref="IModelBinder"/> with string trimming support.
/// </summary>
/// <param name="modelBinder">The decorated model binder.</param>
public sealed class TrimStringModelBinder(IModelBinder modelBinder) : IModelBinder
{
    private sealed class TrimmedValueProvider(IValueProvider valueProvider) : IValueProvider
    {
        /// <inheritdoc/>
        public bool ContainsPrefix(string prefix) => valueProvider.ContainsPrefix(prefix);

        /// <inheritdoc/>
        public ValueProviderResult GetValue(string key)
        {
            var result = valueProvider.GetValue(key);
            return result.Length == 1
                ? new ValueProviderResult(new StringValues(result.FirstValue?.Trim()), result.Culture)
                : result;
        }
    }

    /// <inheritdoc/>
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        bindingContext.ValueProvider = new TrimmedValueProvider(bindingContext.ValueProvider);
        return modelBinder.BindModelAsync(bindingContext);
    }
}

/// <summary>
/// A <see cref="IModelBinderProvider"/> which can trim simple string bindings.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class TrimStringModelBinderProvider : IModelBinderProvider
{
    /// <inheritdoc/>
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ExceptionHelpers.ThrowIfArgumentNull(context, nameof(context));

        bool isSimpleString = !context.Metadata.IsComplexType && context.Metadata.ModelType == typeof(string);
        if (isSimpleString && context.Metadata.DataTypeName != nameof(DataType.Password)) {
            var loggerFactory = context.Services.GetRequiredService<ILoggerFactory>();
            return new TrimStringModelBinder(new SimpleTypeModelBinder(context.Metadata.ModelType, loggerFactory));
        }

        return null;
    }
}
