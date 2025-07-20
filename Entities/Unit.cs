using System.ComponentModel.DataAnnotations;

namespace muhaberat_evrak_yonetim.Entities
{
    public class Unit
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UnitName { get; set; }

        // A unit can have multiple users
        public ICollection<User> Users { get; set; }

        // A unit can have multiple departments
        public ICollection<Department> Departments { get; set; }

        // Documents received by the unit
        public ICollection<Document> ReceivedDocuments { get; set; }
    }
}
