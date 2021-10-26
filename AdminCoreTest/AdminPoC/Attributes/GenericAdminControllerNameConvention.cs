namespace AdminPoC.Attributes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AdminPoC.Controllers;
    using AdminPoC.Extensions;
    using AdminPoC.Models;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using Microsoft.AspNetCore.Mvc.Controllers;

    [AttributeUsage(AttributeTargets.Class)]
    public class GenericAdminControllerNameConvention : Attribute, IControllerModelConvention
    {
        private static IEnumerable<Type> Controllers { get; set; }

        static GenericAdminControllerNameConvention()
        {
            Controllers = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => p.IsSubclassOfRawGeneric(typeof(AdminController<>)))
                .ToList();
        }

        public void Apply(ControllerModel controller)
        {
            if ((!controller.ControllerType.IsGenericType
                 || controller.ControllerType.GetGenericTypeDefinition() != typeof(AdminController<>))
                && !controller.ControllerType.IsSubclassOf(typeof(AdminController<>)))
            {
                return;
            }

            if (Controllers.Any(controllerType => controllerType.IsSubclassOf(controller.ControllerType)))
            {
                return;
            }

            var entityType = controller.ControllerType.GenericTypeArguments[0];
            controller.ControllerName = entityType.Name;
            controller.RouteValues["Controller"] = entityType.Name;
        }
    }

    public class GenericAdminControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            var types = new[] { typeof(Employee), typeof(EmployeeTasks), typeof(Project), typeof(Task) };
            foreach (var entityType in types)
            {
                var typeArgs = new[] { entityType };
                var controllerType = typeof(AdminController<>)
                    .MakeGenericType(typeArgs)
                    .GetTypeInfo();
                feature.Controllers.Add(controllerType);
            }
        }
    }
}