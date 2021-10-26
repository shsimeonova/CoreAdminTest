namespace AdminPoC.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Reflection;
    using AdminPoC.Attributes;
    using AdminPoC.Extensions;
    using AdminPoC.Services;
    using AdminPoC.ViewModels;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.EntityFrameworkCore;

    [GenericAdminControllerNameConvention]
    public class AdminController<T>
        : Controller
        where T : class
    {
        private readonly DbContext db;
        private readonly IEnumerable<DiscoveredDbSetEntityType> dbSetEntityTypes;
        private readonly Type entityType;

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
            this.entityType = typeof(T);
        }

        public virtual IActionResult Index(string id)
            => this.View("../Admin/Index", this.GetIndexViewModel(id));

        [HttpGet]
        public virtual IActionResult Create(string id)
            => this.View("../Admin/Create", this.GetCreateViewModel(id));

        [HttpPost]
        public virtual IActionResult Create(T obj)
        {
            this.ValidateObject(obj);
            this.db.Set<T>()
                .Add(obj);
            this.db.SaveChanges();

            return this.RedirectToAction("Index");
        }

        protected virtual void ValidateObject(T obj)
        {
        }

        private IndexViewModel GetIndexViewModel(string entityName)
        {
            var dbSet = this.db.Set<T>()
                .AsQueryable();

            this.RelatedEntityNames
                .ToList()
                .ForEach(include => { dbSet = dbSet.Include(include); });

            var entityColumns = this.GetEntityColumns(this.entityType);

            return new IndexViewModel
            {
                Entities = dbSet.Reverse(),
                Columns = entityColumns.Concat(this.DynamicColumns),
            };
        }

        private CreateViewModel GetCreateViewModel(string entityName)
        {
            var properties = this.entityType.GetProperties()
                .Where(IsSimplePropertyForCreate)
                .Select(propertyInfo => new InputType
                {
                    Name = propertyInfo.Name,
                    Type = propertyInfo.GetType(),
                });

            var complexProperties = this.entityType
                .GetProperties()
                .Where(propertyInfo =>
                    this.dbSetEntityTypes.Any(
                        et => et.UnderlyingType == propertyInfo.PropertyType))
                .Select(propertyInfo =>
                {
                    var type = propertyInfo.PropertyType;

                    var values = (this.db.Set(type) as IQueryable<object>)
                        .ToList()
                        .Select(x => new SelectListItem
                        {
                            Text = x.ToString(),
                            Value = type.GetPrimaryKeyPropertyInfo()
                                .GetValue(x)
                                .ToString(),
                        })
                        .ToList();
                    return new ComplexInputType
                    {
                        Name = propertyInfo.Name + "Id",
                        Type = type,
                        Values = values,
                    };
                });

            return new CreateViewModel
            {
                EntityName = entityName,
                Properties = properties,
                ComplexProperties = complexProperties,
            };
        }

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
    }
}