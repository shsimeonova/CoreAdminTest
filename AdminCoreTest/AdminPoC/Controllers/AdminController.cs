namespace AdminPoC.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AdminPoC.Extensions;
    using AdminPoC.Models;
    using AdminPoC.ViewModels;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;


    public class AdminController : Controller
    {
        private readonly DbContext db;
        private readonly IEnumerable<DiscoveredDbSetEntityType> dbSetEntityTypes;

        protected virtual IEnumerable<EntityColumn> DynamicColumns
            => new List<EntityColumn>();

        protected virtual IEnumerable<string> StaticColumnNames
            => new HashSet<string>();

        protected virtual IEnumerable<string> RelatedEntityNames
            => Array.Empty<string>();

        public AdminController(
            DbContext db,
            IEnumerable<DiscoveredDbSetEntityType> dbSetEntityTypes)
        {
            this.db = db;
            this.dbSetEntityTypes = dbSetEntityTypes;
        }

        public virtual IActionResult Index(string id)
            => this.View("../Admin/Index", this.GetIndexViewModel(id));

        [HttpGet]
        public virtual IActionResult Create(string id)
            => this.View("../Admin/Create", this.GetCreateViewModel(id));

        [HttpPost]
        public virtual IActionResult Create(IDictionary<string, string> obj)
        {
            var entityName = obj["entityName"];
            var (entityType, dbContextType) = this.GetEntityTypeAndSet(entityName);

            var model = Activator.CreateInstance(entityType);

            foreach (var propertyInfo in entityType.GetProperties()
                .Where(IsDefaultColumnForCreate))
            {
                var stringValue = obj[propertyInfo.Name];
                var propertyType = propertyInfo.PropertyType;
                var value = Convert.ChangeType(stringValue, propertyType);
                propertyInfo.SetValue(model, value);
            }
            
            var dbSet = this.db.GetType()
                .GetMethod("Set", Array.Empty<Type>())
                ?.MakeGenericMethod(entityType)
                .Invoke(this.db, null);

            dbSet.GetType()
                .GetMethod("Add")
                .Invoke(dbSet, new[] { model });

            this.db.SaveChanges();

            return this.Redirect("/projectsadmin");
        }

        private IndexViewModel GetIndexViewModel(string entityName)
        {
            var (entityType, dbContextType) = this.GetEntityTypeAndSet(entityName);

            var dbSet = dbContextType.GetProperties()
                .FirstOrDefault(p => p.Name.ToLower() == entityName)
                ?.GetValue(this.db) as IQueryable<object>;

            this.RelatedEntityNames
                .ToList()
                .ForEach(include => { dbSet = dbSet.Include(include); });

            var entityColumns = this.GetEntityColumns(entityType);

            return new IndexViewModel
            {
                Entities = dbSet,
                Columns = entityColumns.Concat(this.DynamicColumns),
            };
        }

        private CreateViewModel GetCreateViewModel(string entityName)
        {
            var (entityType, dbContextType) = this.GetEntityTypeAndSet(entityName);
            var properties = entityType.GetProperties()
                .Where(IsDefaultColumnForCreate)
                .Select(propertyInfo => new InputType
                {
                    Name = propertyInfo.Name,
                    Type = propertyInfo.GetType(),
                });

            return new CreateViewModel
            {
                Properties = properties,
                EntityName = entityName,
            };
        }

        private static bool IsDefaultColumnForCreate(PropertyInfo propertyInfo)
            => IsDefaultColumn(propertyInfo) && propertyInfo.Name.ToLower() != "id";

        private static bool IsDefaultColumn(PropertyInfo propertyInfo)
        {
            var nonComplexTypes = new HashSet<Type>
            {
                typeof(string),
                typeof(DateTime),
            };

            var propertyType = propertyInfo.PropertyType;

            return propertyType.IsPrimitive
                   || propertyType.IsEnum
                   || nonComplexTypes.Contains(propertyType);
        }

        private bool IsExplicitPropertyFilter(PropertyInfo propertyInfo)
            => this.StaticColumnNames
                .Contains(propertyInfo.Name);

        private IEnumerable<EntityColumn> GetEntityColumns(Type entityType)
        {
            Func<PropertyInfo, bool> filter = this.StaticColumnNames.Any()
                ? this.IsExplicitPropertyFilter
                : IsDefaultColumn;

            return entityType.GetProperties()
                .Where(filter)
                .Select(propertyInfo => new EntityColumn
                {
                    Name = propertyInfo.Name,
                    Func = model => propertyInfo.GetValue(model).ToString(),
                })
                .ToList();
        }

        private (Type, Type ) GetEntityTypeAndSet(string entityName)
        {
            var dbSetEntity = this.dbSetEntityTypes
                .FirstOrDefault(x => string.Equals(x.Name, entityName, StringComparison.CurrentCultureIgnoreCase));
            return (dbSetEntity.UnderlyingType, dbSetEntity.DbContextType);
        }
    }
}