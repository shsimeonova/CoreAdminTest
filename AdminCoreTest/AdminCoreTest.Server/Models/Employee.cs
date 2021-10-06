using System.ComponentModel.DataAnnotations;

namespace AdminCoreTest.Server.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Username { get; set; }
        
        public string Email { get; set; }
        
        public string Phone { get; set; }
    }
}