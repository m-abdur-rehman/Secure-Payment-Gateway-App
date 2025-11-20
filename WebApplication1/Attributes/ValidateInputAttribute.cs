using Microsoft.AspNetCore.Mvc.Filters;
using WebApplication1.Utilities;

namespace WebApplication1.Attributes
{
    /// <summary>
    /// Action filter to validate and sanitize input parameters
    /// </summary>
    public class ValidateInputAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            foreach (var parameter in context.ActionArguments.Values)
            {
                if (parameter == null) continue;

                var properties = parameter.GetType().GetProperties();
                foreach (var property in properties)
                {
                    if (property.PropertyType == typeof(string))
                    {
                        var value = property.GetValue(parameter) as string;
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            // Check for dangerous patterns
                            if (InputSanitizer.ContainsDangerousPatterns(value))
                            {
                                context.ModelState.AddModelError(property.Name, 
                                    "Input contains potentially dangerous characters.");
                                context.Result = new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(
                                    new { error = "Invalid input detected." });
                                return;
                            }

                            // Sanitize the value
                            var sanitized = InputSanitizer.SanitizeString(value);
                            property.SetValue(parameter, sanitized);
                        }
                    }
                }
            }

            base.OnActionExecuting(context);
        }
    }
}

