using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using TadHub.SharedKernel.Api;

namespace TadHub.Infrastructure.Api;

/// <summary>
/// Provider that supplies QueryParametersModelBinder for QueryParameters type.
/// Register in Program.cs: options.ModelBinderProviders.Insert(0, new QueryParametersModelBinderProvider());
/// </summary>
public class QueryParametersModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Metadata.ModelType == typeof(QueryParameters))
        {
            return new BinderTypeModelBinder(typeof(QueryParametersModelBinder));
        }

        return null;
    }
}
