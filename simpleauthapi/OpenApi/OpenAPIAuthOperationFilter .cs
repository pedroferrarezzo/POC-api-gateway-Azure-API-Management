using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OpenAPI
{
    /// <summary>
    ///  <see cref="https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1586"/>
    /// </summary>
    public class OpenAPIAuthOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext ctx)
        {
            var endpointAttributes = ctx.ApiDescription.CustomAttributes();
            var controllerAttributes = (ctx.ApiDescription.ActionDescriptor as ControllerActionDescriptor)?.ControllerTypeInfo.GetCustomAttributes();

            if (!endpointAttributes.Any((a) => a is AllowAnonymousAttribute)
                && (endpointAttributes.Any((a) => a is AuthorizeAttribute)
                    || controllerAttributes?.Any((a) => a is AuthorizeAttribute) == true))
            {
                operation.Security =
                [
                    new() {
                            {
                                new OpenApiSecurityScheme
                                {
                                    Reference = new OpenApiReference
                                    {
                                        Type = ReferenceType.SecurityScheme,
                                        Id = "Bearer",
                                    }
                                }, Array.Empty<string>()
                            }
                        }
                ];
            }
            else
            {
                operation.Security = null;
            }
        }
    }
    
}
