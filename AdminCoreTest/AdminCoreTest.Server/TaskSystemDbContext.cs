﻿using AdminCoreTest.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminCoreTest.Server
{
    public class TaskSystemDbContext : DbContext
    {
        public TaskSystemDbContext(DbContextOptions<TaskSystemDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<Task> Tasks { get; set; }
        
        public DbSet<Employee> Employees { get; set; }
        
        public DbSet<Project> Projects { get; set; }
        
        public DbSet<EmployeeTasks> EmployeeTasks { get; set; }
    }
}