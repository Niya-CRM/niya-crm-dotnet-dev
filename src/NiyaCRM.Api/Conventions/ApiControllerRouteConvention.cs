using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace NiyaCRM.Api.Conventions
{
    /// <summary>
    /// Route convention that adds 'api' prefix to all controllers except Auth and Setup controllers
    /// </summary>
    public class ApiControllerRouteConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            // Skip Auth and Setup controllers - they should be served directly
            if (controller.ControllerName.Equals("Auth", StringComparison.OrdinalIgnoreCase) ||
                controller.ControllerName.Equals("ApplicationSetup", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // Skip controllers that already have explicit route attributes
            if (controller.Selectors.Any(selector => 
                selector.AttributeRouteModel != null && 
                selector.AttributeRouteModel.Template != null &&
                selector.AttributeRouteModel.Template.StartsWith("api/")))
            {
                return;
            }

            // Add 'api' prefix to all other controllers using LINQ
            foreach (var selector in controller.Selectors)
            {
                var model = selector.AttributeRouteModel;
                if (model != null && !string.IsNullOrEmpty(model.Template) && !model.Template.StartsWith("api/"))
                {
                    model.Template = $"api/{model.Template}";
                }
            }
        }
    }
}
