namespace AdminPoC.Controllers
{
    using System.Collections.Generic;
    using AdminPoC.Extensions;
    using AdminPoC.Models;
    using AdminPoC.ViewModels;
    using Microsoft.EntityFrameworkCore;

    public class TaskController
        : AdminController<Task>
    {
        public TaskController(DbContext db, IEnumerable<DbContextEntityType> entityTypes)
            : base(db, entityTypes)
        {
        }

        protected override IEnumerable<EntityColumn> DynamicColumns
            => new[]
            {
                new EntityColumn
                {
                    Name = "Project",
                    Func = obj => (obj as Task).Project.Name,
                }
            };

        protected override IEnumerable<string> RelatedEntityNames
            => new[] { nameof(Task.Project) };
    }
}