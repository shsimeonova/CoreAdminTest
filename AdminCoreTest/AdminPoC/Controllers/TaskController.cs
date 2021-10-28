namespace AdminPoC.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
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
                },
                new EntityColumn
                {
                    Name = "EmployeesCount",
                    Func = obj => (obj as Task).EmployeeTasks.Count(),
                },
                new EntityColumn
                {
                    Name = "Employees",
                    Func = obj => string.Join(", ",
                        (obj as Task).EmployeeTasks.Select(x => x.Employee.Username)),
                }
            };

        protected override IEnumerable<string> RelatedEntityNames
            => new[]
            {
                nameof(Task.Project),
                nameof(Task.EmployeeTasks),
                $"{nameof(Task.EmployeeTasks)}.{nameof(EmployeeTasks.Employee)}",
            };
    }
}