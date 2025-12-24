using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GreenCharge.Data;
using GreenCharge.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace GreenCharge.Controllers
{
    [Authorize]
    public class ReservationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReservationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. REZERVASYON YAPMA SAYFASI (GET)
        [HttpGet]
        public async Task<IActionResult> Create(int stationId)
        {
            var station = await _context.Stations.FindAsync(stationId);

            if (station == null) return NotFound();

            // Pasif istasyon kontrolü
            if (!station.IsActive)
            {
                TempData["Error"] = "Bu istasyon hizmet dışıdır.";
                return RedirectToAction("Index", "Station");
            }

            ViewBag.StationName = station.Name;
            ViewBag.Price = station.PricePerHour;

            // --- GÜNCELLEME 1: Saniye ve Saliseyi Temizleme ---
            // HTML input'un hata vermemesi için saniyeleri sıfırlıyoruz.
            var now = DateTime.Now;
            var cleanDate = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0).AddHours(1);

            var reservation = new Reservation
            {
                StationId = stationId,
                ReservationDate = cleanDate, // Temiz tarih
                DurationMinutes = 60 // Varsayılan 60 dakika
            };

            return View(reservation);
        }

        // 2. REZERVASYONU KAYDET (POST)
        [HttpPost]
        public async Task<IActionResult> Create(Reservation reservation)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Name);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            // İstasyonu en başta çekiyoruz (Hata olursa View'a veri göndermek için lazım)
            var station = await _context.Stations.FindAsync(reservation.StationId);

            if (user != null && station != null)
            {
                reservation.UserId = user.Id;

                // --- GÜNCELLEME 2: Çakışma (Conflict) Kontrolü ---
                // Yeni rezervasyonun bitiş saati
                var newEnd = reservation.ReservationDate.AddMinutes(reservation.DurationMinutes);

                var isConflict = await _context.Reservations.AnyAsync(r =>
                    r.StationId == reservation.StationId && // Aynı istasyon
                    r.Id != reservation.Id && // Kendisi değil

                    // Çakışma Mantığı:
                    // (Mevcut Başlangıç < Yeni Bitiş) VE (Mevcut Bitiş > Yeni Başlangıç)
                    r.ReservationDate < newEnd &&
                    r.ReservationDate.AddMinutes(r.DurationMinutes) > reservation.ReservationDate
                );

                if (isConflict)
                {
                    // Hata mesajı ekle ve formu tekrar göster
                    ModelState.AddModelError("", "Seçtiğiniz tarih ve saat aralığında bu istasyon dolu! Lütfen başka bir saat seçin.");

                    ViewBag.StationName = station.Name;
                    ViewBag.Price = station.PricePerHour;
                    return View(reservation);
                }
                // ------------------------------------------------

                // Fiyat Hesaplama: (Dakika / 60) * Saatlik Ücret
                reservation.TotalPrice = (reservation.DurationMinutes / 60.0) * station.PricePerHour;

                _context.Add(reservation);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(MyReservations));
            }
            return View(reservation);
        }

        // 3. REZERVASYONLARIM (LİSTELEME)
        public async Task<IActionResult> MyReservations()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Name);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            var myReservations = await _context.Reservations
                                            .Include(r => r.Station)
                                            .Where(r => r.UserId == user.Id)
                                            .OrderByDescending(r => r.ReservationDate)
                                            .ToListAsync();

            return View(myReservations);
        }

        // 4. Güncelleme Sayfasını Getir
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            // Sadece kendi rezervasyonunu görebilmeli!
            var userEmail = User.FindFirstValue(ClaimTypes.Name);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            var reservation = await _context.Reservations
                                            .Include(r => r.Station)
                                            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == user.Id);

            if (reservation == null) return NotFound();

            ViewBag.StationName = reservation.Station.Name;
            ViewBag.Price = reservation.Station.PricePerHour;

            return View(reservation);
        }

        // 5. Güncellemeyi Kaydet
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Reservation reservation)
        {
            if (id != reservation.Id) return NotFound();

            var station = await _context.Stations.FindAsync(reservation.StationId);

            // --- GÜNCELLEME 3: Edit içinde de Çakışma Kontrolü ---
            // (Kullanıcı saatini değiştirdiğinde dolu bir yere denk gelmesin)
            var newEnd = reservation.ReservationDate.AddMinutes(reservation.DurationMinutes);
            var isConflict = await _context.Reservations.AnyAsync(r =>
                r.StationId == reservation.StationId &&
                r.Id != reservation.Id &&
                r.ReservationDate < newEnd &&
                r.ReservationDate.AddMinutes(r.DurationMinutes) > reservation.ReservationDate
            );

            if (isConflict)
            {
                ModelState.AddModelError("", "Güncellemek istediğiniz saat aralığı dolu.");
                ViewBag.StationName = station.Name;
                ViewBag.Price = station.PricePerHour;
                return View(reservation);
            }
            // ------------------------------------------------

            // Fiyatı tekrar hesapla
            reservation.TotalPrice = (reservation.DurationMinutes / 60.0) * station.PricePerHour;

            // UserId'yi kaybetmemek için
            var userEmail = User.FindFirstValue(ClaimTypes.Name);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            reservation.UserId = user.Id;

            _context.Update(reservation);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home"); // Anasayfaya dön
        }

        // 6. İptal Onay Sayfası
        [HttpGet]
        public async Task<IActionResult> Cancel(int? id)
        {
            if (id == null) return NotFound();

            var userEmail = User.FindFirstValue(ClaimTypes.Name);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            var reservation = await _context.Reservations
                                            .Include(r => r.Station)
                                            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == user.Id);

            if (reservation == null) return NotFound();

            return View(reservation);
        }

        // 7. İptal İşlemi (Silme)
        [HttpPost, ActionName("Cancel")]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }

        // 8. TÜM REZERVASYONLARI LİSTELE (SADECE ADMIN)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AllReservations()
        {
            var allReservations = await _context.Reservations
                                        .Include(r => r.Station) // İstasyon bilgisini getir
                                        .Include(r => r.User)    // Kullanıcı bilgisini getir (Kim yaptı?)
                                        .OrderByDescending(r => r.ReservationDate)
                                        .ToListAsync();

            return View(allReservations);
        }

        // 9. ADMIN REZERVASYON İPTALİ (POST)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminCancel(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation != null)
            {
                _context.Reservations.Remove(reservation);
                await _context.SaveChangesAsync();
            }
            // İşlem bitince yine tüm listeye dön
            return RedirectToAction(nameof(AllReservations));
        }
    }
}