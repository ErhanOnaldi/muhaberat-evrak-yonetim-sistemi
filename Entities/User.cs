using System.ComponentModel.DataAnnotations;

namespace muhaberat_evrak_yonetim.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public string Title { get; set; }

        // A user can belong to one unit
        public int? UnitId { get; set; }
        public Unit Unit { get; set; }

        // Documents sent by the user
        public ICollection<Document> SentDocuments { get; set; }
    }
}