namespace AdminPoC.ViewModels
{
    using System;
    using System.Linq.Expressions;
    using AdminPoC.Models;

    public class EntityColumn
    {
        public string Name { get; set; }

        public Expression<Func<EntityBase, object>> Func;
    }

    public class EntityAction
    {
        public string Name { get; set; }

        public Action<object> Action { get; set; }
    }
}