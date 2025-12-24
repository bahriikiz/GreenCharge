using Microsoft.AspNetCore.Mvc;
using GreenCharge.Data;
using GreenCharge.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace GreenCharge.Controllers
{
    [Authorize] // Sadece giriş yapmış kişiler
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. PROFİLİM SAYFASI
        public async Task<IActionResult> Index()
        {
            var userEmail = User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            return View(user);
        }

        // 2. GÜNCELLEME (POST)
        [HttpPost]
        public async Task<IActionResult> Update(User model)
        {
            var userEmail = User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user != null)
            {
                user.FullName = model.FullName;
                // Şifre kutusu boş değilse güncelle
                if (!string.IsNullOrEmpty(model.Password))
                {
                    user.Password = model.Password;
                }

                _context.Update(user);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Bilgileriniz güncellendi.";
            }
            return RedirectToAction("Index");
        }

        // 3. HESABIMI SİL (POST)
        [HttpPost]
        public async Task<IActionResult> DeleteAccount()
        {
            var userEmail = User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user != null)
            {
                // Önce kullanıcının rezervasyonlarını silelim (Veritabanı hatası vermesin)
                var reservations = _context.Reservations.Where(r => r.UserId == user.Id);
                _context.Reservations.RemoveRange(reservations);

                // Sonra kullanıcının yorumlarını silelim
                var reviews = _context.Reviews.Where(r => r.UserId == user.Id);
                _context.Reviews.RemoveRange(reviews);

                // En son kullanıcıyı silelim
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                // Çıkış yapıp ana sayfaya at
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}