namespace AdminPoC.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Reflection;
    using AdminPoC.Extensions;
    using AdminPoC.Models;
    using AdminPoC.Services;
    using AdminPoC.ViewModels;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
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
            var converter = new ConverterService();

            foreach (var propertyInfo in entityType.GetProperties()
                .Where(x => IsSimplePropertyForCreate(x)
                            || (x.Name.ToLower().EndsWith("id") && x.Name.ToLower() != "id")))
            {
                var stringValue = obj[propertyInfo.Name];
                var propertyType = propertyInfo.PropertyType;
                var value = converter.ConvertToType(stringValue, propertyType);
                propertyInfo.SetValue(model, value);
            }

            var dbSet = this.GetDbSetByType(entityType);

            dbSet.GetType()
                .GetMethod("Add")
                ?.Invoke(dbSet, new[] { model });

            this.db.SaveChanges();

            return this.Redirect("/projectsadmin");
        }

        private IndexViewModel GetIndexViewModel(string entityName)
        {
            var (entityType, dbContextType) = this.GetEntityTypeAndSet(entityName);

            var dbSet = this.GetDbSetByType(entityType) as IQueryable<object>;

            this.RelatedEntityNames
                .ToList()
                .ForEach(include => { dbSet = dbSet.Include(include); });

            var entityColumns = this.GetEntityColumns(entityType);

            return new IndexViewModel
            {
                Entities = dbSet.Reverse(),
                Columns = entityColumns.Concat(this.DynamicColumns),
            };
        }

        private CreateViewModel GetCreateViewModel(string entityName)
        {
            var (entityType, dbContextType) = this.GetEntityTypeAndSet(entityName);
            var properties = entityType.GetProperties()
                .Where(IsSimplePropertyForCreate)
                .Select(propertyInfo => new InputType
                {
                    Name = propertyInfo.Name,
                    Type = propertyInfo.GetType(),
                });

            var complexProperties = entityType
                .GetProperties()
                .Where(propertyInfo =>
                    this.dbSetEntityTypes.Any(et => et.UnderlyingType == propertyInfo.PropertyType))
                .Select(propertyInfo =>
                {
                    var type = propertyInfo.PropertyType;
                    return new ComplexInputType
                    {
                        Name = propertyInfo.Name + "Id",
                        Type = type,
                        Values = (this.GetDbSetByType(type) as IQueryable<object>)
                            .ToList()
                            .Select(x =>
                            {
                                var primaryKeyType = type
                                    .GetProperties()
                                    .FirstOrDefault(pi => Attribute.IsDefined((MemberInfo)pi, typeof(KeyAttribute)));

                                return new SelectListItem
                                {
                                    Text = x.ToString(),
                                    Value = primaryKeyType.GetValue(x).ToString(),
                                };
                            })
                            .ToList(),
                    };
                });

            return new CreateViewModel
            {
                EntityName = entityName,
                Properties = properties,
                ComplexProperties = complexProperties,
            };
        }

        private object GetDbSetByType(Type type)
            => this.db.GetType()
                .GetMethod("Set", Array.Empty<Type>())
                .MakeGenericMethod(type)
                .Invoke(this.db, null);

        private static bool IsSimpleType(Type t)
        {
            var nonComplexTypes = new HashSet<Type>
            {
                typeof(string),
                typeof(DateTime),
            };

            return nonComplexTypes.Contains(t) || t.IsPrimitive;
        }

        private static bool IsSimplePropertyForCreate(PropertyInfo propertyInfo)
        {
            return (IsSimpleType(propertyInfo.PropertyType)
                    || propertyInfo.PropertyType.IsEnum)
                   && !propertyInfo.Name.ToLower().EndsWith("id");
        }

        private static bool IsDefaultColumn(PropertyInfo propertyInfo)
        {
            var propertyType = propertyInfo.PropertyType;

            return IsSimpleType(propertyType)
                   || propertyType.IsEnum;
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