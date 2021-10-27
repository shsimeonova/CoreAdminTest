namespace AdminPoC.Models
{
    using System.ComponentModel.DataAnnotations.Schema;

    public abstract class EntityBase
    {
        [NotMapped]
        public abstract object PrimaryKey { get; }
    }
}