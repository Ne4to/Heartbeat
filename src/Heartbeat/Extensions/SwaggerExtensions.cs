using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;

using System.Reflection;

namespace Heartbeat.Host.Extensions;

internal static class SwaggerExtensions
{
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        return services
            .AddEndpointsApiExplorer()
            .AddSwaggerGen(options =>
            {
                options.SupportNonNullableReferenceTypes();
                options.CustomOperationIds(
                    (Func<ApiDescription, string>)(e => e.ActionDescriptor.RouteValues["action"]));
                // options.MarkNonNullablePropertiesAsRequired();
                // options.EnableAnnotations();
                options.SchemaFilter<AnnotationsSchemaFilter>();
                options.ParameterFilter<AnnotationsParameterFilter>();
                options.RequestBodyFilter<AnnotationsRequestBodyFilter>();
                options.OperationFilter<AnnotationsOperationFilter>();
                options.DocumentFilter<AnnotationsDocumentFilter>();

                options.AddServer(new OpenApiServer() { Url = "/" });

                options.SwaggerDoc("Heartbeat",
                    new OpenApiInfo()
                    {
                        Title = "Heartbeat",
                        Description = "Heartbeat contract",
                        Version = "0.1.0",
                        Contact = new OpenApiContact { Name = "Heartbeat" }
                    });

                options.MarkNonNullablePropertiesAsRequired();
            });
    }

    // https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/2036#issuecomment-894015122
    private static void MarkNonNullablePropertiesAsRequired(this SwaggerGenOptions options)
    {
        options.SchemaFilter<RequireNonNullablePropertiesSchemaFilter>();
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal class CombineTagsToGlobalSectionDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var tags = swaggerDoc
            .Paths.SelectMany(
                p => p.Value
                    .Operations.SelectMany(
                        o => o.Value
                            .Tags.Select(t => (TagName: t.Name, TagDescription: t.Description))))
            .Distinct()
            .Select(t => new OpenApiTag { Name = t.TagName, Description = t.TagDescription })
            .ToArray();

        swaggerDoc.Tags = tags.Any()
            ? tags
            : swaggerDoc.Tags;
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class RequireNonNullablePropertiesSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema model, SchemaFilterContext context)
    {
        FixNullableProperties(model, context);

        var additionalRequiredProps = model.Properties
            .Where(x => !x.Value.Nullable && !model.Required.Contains(x.Key))
            .Select(x => x.Key);

        foreach (var propKey in additionalRequiredProps)
        {
            model.Required.Add(propKey);
        }
    }

    private static void FixNullableProperties(OpenApiSchema schema, SchemaFilterContext context)
    {
        foreach (var property in schema.Properties)
        {
            if (property.Value.Reference != null)
            {
                var field = context.Type
                    .GetMembers(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(x =>
                        string.Equals(x.Name, property.Key, StringComparison.InvariantCultureIgnoreCase));

                if (field != null)
                {
                    var fieldType = field switch
                    {
                        FieldInfo fieldInfo => fieldInfo.FieldType,
                        PropertyInfo propertyInfo => propertyInfo.PropertyType,
                        _ => throw new NotSupportedException(),
                    };

                    property.Value.Nullable = fieldType.IsValueType
                        ? Nullable.GetUnderlyingType(fieldType) != null
                        : !field.IsNonNullableReferenceType();
                }
            }
        }
    }
}