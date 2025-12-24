using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GreenCharge.Data;
using GreenCharge.Models;
using Microsoft.AspNetCore.Authorization; 

namespace GreenCharge.Controllers
{
    public class StationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. LİSTELEME (READ)
        public async Task<IActionResult> Index()
        {
            var stations = await _context.Stations.ToListAsync();
            return View(stations);
        }

        // 2. EKLEME SAYFASINI GETİR (CREATE - GET)
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // 3. EKLEME İŞLEMİNİ YAP (CREATE - POST)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Station station)
        {
            if (ModelState.IsValid)
            {
                _context.Add(station);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(station);
        }

        // --- Durum Değiştirme ---
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var station = await _context.Stations.FindAsync(id);
            if (station != null)
            {
                station.IsActive = !station.IsActive;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // 4. SİLME ONAY SAYFASINI GETİR (GET)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var station = await _context.Stations.FirstOrDefaultAsync(m => m.Id == id);
            if (station == null) return NotFound();

            return View(station);
        }

        // 5. SİLME İŞLEMİNİ GERÇEKLEŞTİR (POST)
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var station = await _context.Stations.FindAsync(id);
            if (station != null)
            {
                _context.Stations.Remove(station);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // 6. DÜZENLEME SAYFASINI GETİR (GET)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var station = await _context.Stations.FindAsync(id);
            if (station == null) return NotFound();

            return View(station);
        }

        // 7. GÜNCELLEMEYİ KAYDET (POST)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Station station)
        {
            if (id != station.Id) return NotFound();

            var existingStation = await _context.Stations.FindAsync(id);

            if (existingStation == null) return NotFound();

            // Verileri manuel eşleştiriyoruz
            existingStation.Name = station.Name;
            existingStation.City = station.City;
            existingStation.Address = station.Address;
            existingStation.ChargeType = station.ChargeType;
            existingStation.PricePerHour = station.PricePerHour;
            existingStation.IsActive = station.IsActive;
            existingStation.LocationCode = station.LocationCode;

            try
            {
                _context.Update(existingStation);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Stations.Any(e => e.Id == station.Id))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Index));
        }

        // 8. DETAY SAYFASI
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var station = await _context.Stations
                .Include(s => s.Reviews)
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (station == null) return NotFound();

            return View(station);
        }

        // 9. YORUM EKLEME (POST)
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int stationId, int rating, string comment)
        {
            var userEmail = User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user != null)
            {
                var review = new Review
                {
                    StationId = stationId,
                    UserId = user.Id,
                    Rating = rating,
                    Comment = comment,
                    CreatedAt = DateTime.Now
                };

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = stationId });
        }
    }
}