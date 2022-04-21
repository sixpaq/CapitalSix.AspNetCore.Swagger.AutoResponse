using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CapitalSix.AspNetCore.Swagger.AutoResponse;

/// <summary>
/// This filter will automatically add response descriptions
/// for HTTP status codes 400 and 500
/// </summary>
public class DefaultOperationResponseFilter : IOperationFilter
{
    /// <summary>
    /// Add response descriptions for HTTP status codes 400 and 500
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="context"></param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context?.MethodInfo?.DeclaringType == null) return;

        var hasAuthorize =
            context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() ||
            context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();

        if (context.MethodInfo.CustomAttributes.Any(attribute =>
                attribute.AttributeType == typeof(RequestLimitAttribute)))
        {
            operation.Responses.Add("429", new OpenApiResponse {Description = "Too many requests"});
        }

        if (context.MethodInfo.CustomAttributes.Any(attribute =>
                attribute.AttributeType == typeof(ValidateReferrerAttribute)))
        {
            operation.Responses.Add("417", new OpenApiResponse {Description = "Expected referrer header missing"});
        }

        var responseType = typeof(ProblemDetails);
        var problemTypeSchema = context.SchemaGenerator.GenerateSchema(responseType, context.SchemaRepository);
        
        operation.Responses.Add("400", new OpenApiResponse()
        {
            Description = "Bad Request",
            Content = new Dictionary<string, OpenApiMediaType>()
            {
                ["application/problem+json"] = new OpenApiMediaType
                {
                    Schema = problemTypeSchema
                },
                ["application/json"] = new OpenApiMediaType
                {
                    Schema = problemTypeSchema
                }
            }
        });

        operation.Responses.Add("500", new OpenApiResponse()
        {
            Description = "Internal Server Error",
            Content = new Dictionary<string, OpenApiMediaType>()
            {
                ["application/problem+json"] = new OpenApiMediaType
                {
                    Schema = problemTypeSchema
                },
                ["application/json"] = new OpenApiMediaType
                {
                    Schema = problemTypeSchema
                }
            }
        });
        
        if (!hasAuthorize) return;
            
        operation.Responses.Add("401", new OpenApiResponse {Description = "Unauthorized access"});
        operation.Responses.Add("403", new OpenApiResponse {Description = "Insufficient permissions to access this method"});
    }
}