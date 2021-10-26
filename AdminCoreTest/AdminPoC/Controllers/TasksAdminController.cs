namespace AdminPoC.Controllers
{
    using System.Collections.Generic;
    using AdminPoC.Extensions;
    using AdminPoC.Models;
    using AdminPoC.ViewModels;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    public class TasksAdminController
        : AdminController<Task>
    {
        public TasksAdminController(DbContext db, IEnumerable<DiscoveredDbSetEntityType> dbSetEntityTypes)
            : base(db, dbSetEntityTypes)
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

        public override IActionResult Index(string id)
            => base.Index("tasks");
    }
}