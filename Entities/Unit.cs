using System.ComponentModel.DataAnnotations;

namespace muhaberat_evrak_yonetim.Entities
{
    public class Unit
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UnitName { get; set; }

        public ICollection<User> Users { get; set; }

        public ICollection<Department> Departments { get; set; }

        public ICollection<Document> ReceivedDocuments { get; set; }
    }
}
