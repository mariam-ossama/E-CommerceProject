using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;

public class SwaggerFileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        foreach (var parameter in operation.Parameters)
        {
            // Check if the parameter is an IFormFile
            if (parameter.Name.Equals("imageFile", StringComparison.OrdinalIgnoreCase))
            {
                // Set the parameter to be of type file and form data
                parameter.In = ParameterLocation.Query;  // Use Query for file uploads in the new version of Swashbuckle
                parameter.Schema.Type = "string";
                parameter.Schema.Format = "binary";  // Indicates a file upload
            }
        }
    }
}
