using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Linq.Expressions;

namespace CleanArchitecture.WebApi.Extensions
{
    /// <summary>
    /// Prevents Swagger from generating schema for Expression types (causes hang/timeout).
    /// </summary>
    public class SwaggerExpressionSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == null) return;

            if (IsExpressionType(context.Type))
            {
                schema.Type = "object";
                schema.Properties?.Clear();
                schema.Description = "Filter expression (not documented in OpenAPI).";
            }
        }

        private static bool IsExpressionType(Type type)
        {
            if (type == null) return false;
            if (type.FullName != null && type.FullName.StartsWith("System.Linq.Expressions.")) return true;
            if (typeof(Expression).IsAssignableFrom(type)) return true;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Expression<>)) return true;
            return false;
        }
    }
}
