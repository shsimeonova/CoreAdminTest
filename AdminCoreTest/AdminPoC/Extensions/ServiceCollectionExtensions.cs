namespace AdminPoC.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;

    public class DbContextEntityType
    {
        public Type EntityType { get; internal set; }
    }

    public static class ServiceCollectionExtensions
    {
        public static void AddAdmin(this IServiceCollection services)
        {
            FindDbContexts(services);

            services.AddControllersWithViews();
        }

        private static void FindDbContexts(IServiceCollection services)
        {
            var servicesToRemove = services
                .Where(s => s.ServiceType == typeof(DbContextEntityType))
                .ToList();

            servicesToRemove
                .ForEach(s => services.Remove(s));

            services
                .ToList()
                .Where(s => s.ImplementationType != null
                            || s.ImplementationType?.IsSubclassOf(typeof(DbContext)) == true)
                .SelectMany(s => s.ImplementationType?.GetProperties()
                    .Where(p => p.PropertyType.IsGenericType
                                && p.PropertyType.Name.StartsWith("DbSet"))
                    .Select(p => new DbContextEntityType
                    {
                        EntityType = p.PropertyType.GenericTypeArguments.First(),
                    }))
                .ToList()
                .ForEach(s => services.AddTransient(_ => s));
        }
    }
}
