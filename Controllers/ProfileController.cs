using Microsoft.AspNetCore.Mvc;
using GreenCharge.Data;
using GreenCharge.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace GreenCharge.Controllers
{
    [Authorize] // Sadece giriş yapmış kişiler erişebilir
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. PROFİL SAYFASI (GÖRÜNTÜLEME)
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Çerezden giriş yapan kullanıcının e-postasını al
            var userEmail = User.Identity?.Name;
            if (userEmail == null) return RedirectToAction("Login", "Account");

            // Veritabanından bu kişiyi bul
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null) return NotFound();

            return View(user);
        }

        // 2. GÜNCELLEME İŞLEMİ (KAYDETME)
        [HttpPost]
        public async Task<IActionResult> Update(GreenCharge.Models.User model)
        {
            // Güvenlik için: Formdan gelen ID yerine, giriş yapan kişinin verisini veritabanından tekrar çekiyoruz
            var userEmail = User.Identity?.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user != null)
            {
                // Sadece Ad Soyad ve Şifre güncellenebilir (Rol ve Email sabit kalmalı)
                user.FullName = model.FullName;

                // Eğer kullanıcı şifre kutusunu boş bırakmadıysa şifreyi de güncelle
                if (!string.IsNullOrEmpty(model.Password))
                {
                    user.Password = model.Password;
                }

                _context.Update(user);
                await _context.SaveChangesAsync(); // <-- Veritabanına yazan sihirli komut

                TempData["Success"] = "Bilgileriniz başarıyla güncellendi.";
            }

            return RedirectToAction("Index");
        }
    }
}