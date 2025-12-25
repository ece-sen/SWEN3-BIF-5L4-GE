using System.ComponentModel.DataAnnotations;

namespace Paperless.Models
{
    public class Favorite
    {
        [Key]
        public int Id { get; set; }
        // Foreign key to Document
        public int DocumentId { get; set; }
        // Navigation property to Document
        public Document? Document { get; set; }

    }
}