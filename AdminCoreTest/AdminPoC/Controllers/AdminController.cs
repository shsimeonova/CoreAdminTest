namespace AdminPoC.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AdminPoC.Attributes;
    using AdminPoC.Extensions;
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
        private readonly ISet<Type> entityTypes;
        private readonly Type entityType;

        protected virtual IEnumerable<EntityColumn> DynamicColumns
            => new List<EntityColumn>();

        protected virtual IEnumerable<string> StaticColumnNames
            => new HashSet<string>();

        protected virtual IEnumerable<string> RelatedEntityNames
            => Array.Empty<string>();

        public AdminController(
            DbContext db,
            IEnumerable<DbContextEntityType> entityTypes)
        {
            this.db = db;
            this.entityTypes = entityTypes.Select(x => x.EntityType)
                .ToHashSet();
            this.entityType = typeof(T);
        }

        public virtual IActionResult Index()
            => this.View("../Admin/Index", this.GetIndexViewModel());

        [HttpGet]
        public virtual IActionResult Create()
            => this.View("../Admin/Create", this.GetCreateViewModel());

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

        private IndexViewModel GetIndexViewModel()
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

        private CreateViewModel GetCreateViewModel()
            => new()
            {
                Properties = this.entityType.GetProperties()
                    .Where(IsSimplePropertyForCreate)
                    .Select(propertyInfo => new InputType
                    {
                        Name = propertyInfo.Name,
                        Type = propertyInfo.GetType(),
                    }),
                ComplexProperties = this.entityType
                    .GetProperties()
                    .Where(propertyInfo =>
                        this.entityTypes.Contains(propertyInfo.PropertyType))
                    .Select(propertyInfo => new ComplexInputType
                    {
                        Name = propertyInfo.Name + "Id",
                        Type = propertyInfo.PropertyType,
                        Values = (this.db.Set(propertyInfo.PropertyType) as IQueryable<object>)
                            .Select(x => new SelectListItem
                            {
                                Text = x.ToString(),
                                Value = propertyInfo.PropertyType
                                    .GetPrimaryKeyPropertyInfo()
                                    .GetValue(x)
                                    .ToString(),
                            }),
                    }),
            };

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