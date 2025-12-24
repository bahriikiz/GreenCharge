using System.ComponentModel.DataAnnotations.Schema; // Bunu eklemeyi unutma!
namespace GreenCharge.Models
{
    public class Reservation
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int StationId { get; set; }

        public DateTime ReservationDate { get; set; }

        public int DurationMinutes { get; set; }

        public double TotalPrice { get; set; }

        [ForeignKey("UserId")]

        public virtual User User { get; set; }

        [ForeignKey("StationId")]
        public virtual Station Station { get; set; }
    }
}