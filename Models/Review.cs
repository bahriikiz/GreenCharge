using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenCharge.Models
{
    public class Review
    {
        public int Id { get; set; }

        public int UserId { get; set; } // Kim yorum yaptı?
        public int StationId { get; set; } // Hangi istasyona?

        [Range(1, 5)]
        public int Rating { get; set; } // 1 ile 5 arası puan

        public string? Comment { get; set; } // Yorum (İsteğe bağlı)
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // İlişkiler
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("StationId")]
        public virtual Station Station { get; set; }
    }
}