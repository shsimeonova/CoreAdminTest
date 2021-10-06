using System.ComponentModel.DataAnnotations;

namespace AdminCoreTest.Server.Models
{
    public class EmployeeTasks
    {
        [Key]
        public int Id { get; set; }
        
        public int EmployeeId { get; set; }
        
        public Employee Employee { get; set; }
        
        public int TaskId { get; set; }
        
        public Task Task { get; set; }
    }
}