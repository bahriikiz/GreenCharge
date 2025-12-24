using Microsoft.AspNetCore.Mvc;
using GreenCharge.Models;
using GreenCharge.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Diagnostics;

namespace GreenCharge.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context; // Veritabanı bağlantısı

        // Constructor: Logger ve Veritabanı servisini içeri alıyoruz
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Anasayfa (Index): Giriş yapan kişinin rezervasyonlarını gösterir
        public async Task<IActionResult> Index()
        {
            // Kullanıcı giriş yapmış mı?
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Name);

                if (!string.IsNullOrEmpty(userEmail))
                {
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

                    if (user != null)
                    {
                        // Kullanıcının rezervasyonlarını getir
                        var myReservations = await _context.Reservations
                                                .Include(r => r.Station)
                                                .Where(r => r.UserId == user.Id)
                                                .OrderByDescending(r => r.ReservationDate)
                                                .ToListAsync();

                        return View(myReservations);
                    }
                }
            }

            // Giriş yapmamışsa boş liste gönder (Hata almamak için)
            return View(new List<Reservation>());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}