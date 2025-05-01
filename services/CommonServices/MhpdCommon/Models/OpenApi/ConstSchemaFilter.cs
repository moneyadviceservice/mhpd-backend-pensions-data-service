using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MhpdCommon.Models.OpenApi;

public class ConstSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.MemberInfo?.GetCustomAttributes(typeof(FixedValueAttribute), false)
            .FirstOrDefault() is FixedValueAttribute fixedAttr && fixedAttr.ExpectedValue != null)
        {
            schema.Enum = [new OpenApiString(fixedAttr.ExpectedValue.ToString())];
            schema.Description += $" (Constant value: {fixedAttr.ExpectedValue})";
        }
    }
}
