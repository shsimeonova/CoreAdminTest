namespace AdminPoC.Controllers
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using AdminPoC.Extensions;
    using AdminPoC.Models;
    using AdminPoC.ViewModels;
    using Microsoft.EntityFrameworkCore;

    public class ProjectController
        : AdminController<Project>
    {
        public ProjectController(DbContext db, IEnumerable<DbContextEntityType> entityTypes)
            : base(db, entityTypes)
        {
        }

        protected override IEnumerable<EntityColumn> DynamicColumns
            => new[]
            {
                new EntityColumn
                {
                    Name = "Tasks Counts",
                    Func = o => (o as Project).Tasks.Count.ToString(),
                }
            };

        protected override IEnumerable<string> RelatedEntityNames
            => new[] { "Tasks" };

        protected override void ValidateObject(Project obj)
        {
            ValidateProjectName(obj.Name);
        }

        private static void ValidateProjectName(string value)
        {
            if (value.Contains('@'))
            {
                throw new ValidationException("Project name cannot contain '@'");
            }
        }
    }
}