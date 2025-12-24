using Microsoft.AspNetCore.Mvc;
using GreenCharge.Data;
using GreenCharge.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GreenCharge.Controllers
{
    [Authorize(Roles = "Admin")] // Sadece Admin girebilir!
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. KULLANICI LİSTESİ
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        // 2. KULLANICIYI SİL (POST)
        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                // İlişkili verileri temizle
                var reservations = _context.Reservations.Where(r => r.UserId == id);
                _context.Reservations.RemoveRange(reservations);

                var reviews = _context.Reviews.Where(r => r.UserId == id);
                _context.Reviews.RemoveRange(reviews);

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // 3. ROL/BİLGİ GÜNCELLEME (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        // 4. ROL/BİLGİ GÜNCELLEME (POST)
        [HttpPost]
        public async Task<IActionResult> Edit(User model)
        {
            var user = await _context.Users.FindAsync(model.Id);
            if (user != null)
            {
                user.FullName = model.FullName;
                user.Email = model.Email;
                user.Role = model.Role; // Admin rolünü buradan değiştirebilir

                if (!string.IsNullOrEmpty(model.Password))
                {
                    user.Password = model.Password;
                }

                _context.Update(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}