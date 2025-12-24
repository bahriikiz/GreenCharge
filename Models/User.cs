namespace GreenCharge.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        // Ödevdeki "Rol Yönetimi" şartı için (Örn: "Admin", "Member")
        public string Role { get; set; } = "Member";
    }
}