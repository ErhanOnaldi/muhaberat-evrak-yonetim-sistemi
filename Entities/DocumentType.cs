using System.Collections.Generic;

namespace muhaberat_evrak_yonetim.Entities
{
    public class DocumentType
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public ICollection<Document> Documents { get; set; }
    }
}
