namespace GreenCharge.Models
{
    public class Station
    {
        public int Id { get; set; }
        public string Name { get; set; }       // İstasyon Adı
        public string City { get; set; }       // Şehir
        public string Address { get; set; }    // Adres
        public string ChargeType { get; set; } // AC/DC
        public double PricePerHour { get; set; }// Saatlik Ücret
        public bool IsActive { get; set; }     // Aktif mi?
        public virtual ICollection<Review>? Reviews { get; set; }  //Yorum
        public string? LocationCode { get; set; } //Konum
    }
}