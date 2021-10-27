namespace AdminPoC.Controllers
{
    using System;
    using System.Collections.Generic;
    using AdminPoC.Extensions;
    using AdminPoC.Models;
    using AdminPoC.ViewModels;
    using Microsoft.AspNetCore.Mvc;
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

        protected override IEnumerable<EntityAction> CustomActions
            => new[]
            {
                new EntityAction
                {
                    Name = "This",
                    Action = "This"
                },
                new EntityAction
                {
                    Name = "That",
                    Action = "That"
                }
            };

        protected override IEnumerable<Func<Project, ValidatorResult>> EntityValidators
            => new Func<Project, ValidatorResult>[]
            {
                ValidateProjectName,
                ValidateProjectNameLength,
            };

        public IActionResult This()
            => this.Ok("IT works!");

        public IActionResult That(string id)
            => this.Ok($"IT works with ID: {id}!");

        private static ValidatorResult ValidateProjectName(Project pr)
            => pr.Name.Contains("@")
                ? ValidatorResult.Error("Project name cannot contain '@'")
                : ValidatorResult.Success();

        private static ValidatorResult ValidateProjectNameLength(Project pr)
            => pr.Name.Length < 20
                ? ValidatorResult.Success()
                : ValidatorResult.Error("Project name must be below 20 characters");
    }
}