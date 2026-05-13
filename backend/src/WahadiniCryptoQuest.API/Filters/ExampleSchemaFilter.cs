using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using WahadiniCryptoQuest.Core.DTOs.Progress;

namespace WahadiniCryptoQuest.API.Filters;

/// <summary>
/// Schema filter to add example values to Swagger documentation
/// </summary>
public class ExampleSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(UpdateProgressDto))
        {
            schema.Example = new OpenApiObject
            {
                ["watchPosition"] = new OpenApiInteger(245),
            };
        }
        else if (context.Type == typeof(ProgressDto))
        {
            schema.Example = new OpenApiObject
            {
                ["lessonId"] = new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                ["lastWatchedPosition"] = new OpenApiInteger(245),
                ["completionPercentage"] = new OpenApiDouble(81.67),
                ["isCompleted"] = new OpenApiBoolean(true),
                ["completedAt"] = new OpenApiString("2025-11-16T14:30:00Z"),
                ["totalWatchTime"] = new OpenApiInteger(320)
            };
        }
        else if (context.Type == typeof(UpdateProgressResultDto))
        {
            schema.Example = new OpenApiObject
            {
                ["success"] = new OpenApiBoolean(true),
                ["completionPercentage"] = new OpenApiDouble(81.67),
                ["isNewlyCompleted"] = new OpenApiBoolean(true),
                ["pointsAwarded"] = new OpenApiInteger(50),
                ["totalPoints"] = new OpenApiInteger(850)
            };
        }
    }
}
