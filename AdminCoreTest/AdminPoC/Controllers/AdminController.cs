namespace AdminPoC.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using AdminPoC.Attributes;
    using AdminPoC.Extensions;
    using AdminPoC.Models;
    using AdminPoC.ViewModels;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.EntityFrameworkCore;

    public class ValidatorResult
    {
        public bool IsValid { get; set; }

        public string Message { get; set; }

        public static ValidatorResult Success()
            => new()
            {
                IsValid = true,
            };

        public static ValidatorResult Error(string message)
            => new()
            {
                IsValid = false,
                Message = message,
            };
    }

    [GenericAdminControllerNameConvention]
    public class AdminController<T>
        : Controller
        where T : EntityBase
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

        protected virtual IEnumerable<Func<T, ValidatorResult>> EntityValidators
            => Array.Empty<Func<T, ValidatorResult>>();

        private static IEnumerable<EntityAction> DefaultActions
            => new[]
            {
                new EntityAction
                {
                    Name = "Delete",
                    Action = "Delete",
                }
            };

        protected virtual IEnumerable<EntityAction> CustomActions
            => Array.Empty<EntityAction>();

        private IEnumerable<EntityAction> Actions =>
            DefaultActions.Concat(this.CustomActions);

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
            var errors = this.EntityValidators
                .Select(v => v(obj))
                .Where(x => !x.IsValid)
                .Select(x => x.Message)
                .ToList();

            if (errors.Any())
            {
                throw new Exception(string.Join(", ", errors));
            }

            this.db.Set<T>()
                .Add(obj);
            this.db.SaveChanges();

            return this.RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Delete(string id)
        {
            var lambda = this.GetObjectById(id);

            var obj = this.db.Set<T>()
                .FirstOrDefault(lambda);

            this.db.Set<T>()
                .Remove(obj);
            this.db.SaveChanges();
            return this.RedirectToAction("Index");
        }

        private Expression<Func<T, bool>> GetObjectById(string id)
        {
            var primaryKeyProp = this.entityType
                .GetPrimaryKeyPropertyInfo();
            var parameter = Expression.Parameter(typeof(EntityBase), "model");
            var convertedParameter = Expression.Convert(
                parameter,
                this.entityType
            );

            var memberAccess = Expression.MakeMemberAccess(
                convertedParameter,
                primaryKeyProp
            );

            var cast = Expression.Call(
                Expression.Convert(memberAccess, typeof(object)),
                typeof(object).GetMethod("ToString")!);

            var idMember = Expression.Convert(
                Expression.Constant(id),
                typeof(string));

            var equals = Expression.Equal(
                cast,
                idMember
            );

            return Expression.Lambda<Func<T, bool>>(
                equals,
                parameter);
        }

        private IndexViewModel GetIndexViewModel()
        {
            var dbSet = this.db.Set<T>()
                .AsQueryable();

            this.RelatedEntityNames
                .ToList()
                .ForEach(include => { dbSet = dbSet.Include(include); });

            var entityColumns = this.GetEntityColumns();

            return new IndexViewModel
            {
                Entities = dbSet.Reverse(),
                Columns = entityColumns.Concat(this.DynamicColumns),
                Actions = this.Actions,
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
                        Values = (this.db.Set(propertyInfo.PropertyType) as IQueryable<object>)!
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

        private IEnumerable<EntityColumn> GetEntityColumns()
        {
            Func<PropertyInfo, bool> filter = this.StaticColumnNames.Any()
                ? this.IsExplicitPropertyFilter
                : IsDefaultColumn;

            return this.entityType
                .GetProperties()
                .Where(filter)
                .Select(propertyInfo =>
                {
                    var parameter = Expression.Parameter(typeof(EntityBase), "model");

                    var convertedParameter = Expression.Convert(
                        parameter,
                        this.entityType
                    );

                    var memberAccess = Expression.MakeMemberAccess(
                        convertedParameter,
                        propertyInfo
                    );

                    var cast = Expression.Convert(
                        memberAccess,
                        typeof(object));

                    var lambda = Expression.Lambda<Func<EntityBase, object>>(
                        cast,
                        parameter);
                    return new EntityColumn
                    {
                        Name = propertyInfo.Name,
                        Func = lambda,
                    };
                })
                .ToList();
        }
    }
}