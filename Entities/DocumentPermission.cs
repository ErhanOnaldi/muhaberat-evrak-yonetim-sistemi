using muhaberat_evrak_yonetim.Enums;

namespace muhaberat_evrak_yonetim.Entities
{
    public class DocumentPermission
    {
        public int Id { get; set; }

        public int DocumentId { get; set; }
        public Document Document { get; set; }

        public int? UserId { get; set; }
        public User User { get; set; }

        public int? RoleId { get; set; }
        public Role Role { get; set; }

        public PermissionType PermissionType { get; set; }
    }
}
