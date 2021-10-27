namespace AdminPoC.Models
{
    using System.ComponentModel.DataAnnotations;

    public class Employee
        : EntityBase
    {
        public override object PrimaryKey
            => this.Id;
        [Key] public int Id { get; set; }

        [Required] public string Username { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public override string ToString()
            => this.Username;
    }
}