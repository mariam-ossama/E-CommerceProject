using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class SwaggerHomePageImageUploadFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.ApiDescription.RelativePath.Contains("add-homepage-image") &&
            operation.RequestBody != null)
        {
            operation.RequestBody.Content["multipart/form-data"] = new OpenApiMediaType
            {
                Schema = new OpenApiSchema
                {
                    Type = "object",
                    Properties =
                    {
                        ["imageFile"] = new OpenApiSchema
                        {
                            Type = "string",
                            Format = "binary"
                        },
                        ["imageType"] = new OpenApiSchema
                        {
                            Type = "string"
                        },
                        ["displayOrder"] = new OpenApiSchema
                        {
                            Type = "integer",
                            Format = "int32"
                        }
                    },
                    Required = new HashSet<string> { "imageFile", "imageType", "displayOrder" }
                }
            };
        }
    }
}
