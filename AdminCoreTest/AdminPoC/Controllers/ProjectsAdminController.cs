namespace AdminPoC.Controllers
{
    using System.Collections.Generic;
    using AdminPoC.Extensions;
    using AdminPoC.Models;
    using AdminPoC.ViewModels;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    public class ProjectsAdminController
        : AdminController
    {
        public ProjectsAdminController(DbContext db, IEnumerable<DiscoveredDbSetEntityType> dbSetEntityTypes)
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
    }
}