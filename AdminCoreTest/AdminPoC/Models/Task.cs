﻿namespace AdminPoC.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Task
        : EntityBase
    {
        public override object PrimaryKey
            => this.Id;

        [Key] public int Id { get; set; }

        public string Name { get; set; }

        public DateTime OpenDate { get; set; }

        public DateTime DueDate { get; set; }

        [Required] 
        public TaskExecutionType ExecutionType { get; set; }

        [Required] 
        public TaskLabelType LabelType { get; set; }

        public int ProjectId { get; set; }

        public Project Project { get; set; }
        
        public IEnumerable<EmployeeTasks> EmployeeTasks { get; set; }

        public override string ToString()
            => this.Name;
    }
}