namespace AdminPoC.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using AdminPoC.Extensions;
    using AdminPoC.Models;
    using AdminPoC.ViewModels;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    public class ProjectController
        : AdminController<Project>
    {
        public ProjectController(DbContext db, IEnumerable<DiscoveredDbSetEntityType> dbSetEntityTypes)
            : base(db, dbSetEntityTypes)
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

        public override IActionResult Index(string id)
            => base.Index("projects");

        public override IActionResult Create(string id)
            => base.Create("projects");

        protected override void ValidateProperty(string propertyName, object value)
        {
            if (string.Equals(propertyName, nameof(Project.Name), StringComparison.CurrentCultureIgnoreCase))
            {
                this.ValidateProjectName(value.ToString());
            }
        }

        private void ValidateProjectName(string value)
        {
            if (value.Contains('@'))
            {
                throw new ValidationException("Project name cannot contain '@'");
            }
        }
    }
}