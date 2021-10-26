using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AdminPoC
{
    using System;
    using System.Linq;
    using AdminCoreTest.Server;
    using AdminPoC.Extensions;
    using AdminPoC.Models;
    using Microsoft.EntityFrameworkCore;

    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<TaskSystemDbContext>(options =>
                options.UseInMemoryDatabase("AdminCoreTest.TaskSystem"));

            services.AddTransient<DbContext, TaskSystemDbContext>();

            services.AddAdmin();

            services.AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            this.FillData(app);
        }

        public void FillData(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope();
            var context = scope?.ServiceProvider.GetRequiredService<TaskSystemDbContext>();

            context.Projects.AddRange(
                Enumerable.Range(1, 1 << 10)
                    .Select(index => new Project
                    {
                        Name = $"Project {index}",
                        OpenDate = DateTime.Now.AddDays(index % 100),
                        DueDate = DateTime.Now.AddDays(index % 100 + 15)
                    }));
            context.Tasks.AddRange(
                Enumerable.Range(1, 1 << 10)
                    .Select(index => new Task()
                    {
                        Name = $"Task {index}",
                        OpenDate = DateTime.Now.AddDays(index % 100),
                        DueDate = DateTime.Now.AddDays(index % 100 + 15),
                        ExecutionType = TaskExecutionType.Finished,
                        LabelType = TaskLabelType.Hibernate,
                        ProjectId = index % 50 + 1
                    }));

            context.Employees.AddRange(
                Enumerable.Range(1, 10)
                    .Select(index => new Employee()
                    {
                        Username = $"Employee {index}",
                        Email = $"employee{index}@gmail.com",
                        Phone = "123123 123  123",
                    }));
            context.SaveChanges();
        }
    }
}