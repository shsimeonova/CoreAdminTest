using System;
using System.ComponentModel.DataAnnotations;

namespace AdminCoreTest.Server.Models
{
    public class Project
    {
        [Key]
        public int Id { get; set; }
        
        public string Name { get; set; }
        
        public DateTime OpenDate { get; set; }
        
        public DateTime DueDate { get; set; }
    }
}