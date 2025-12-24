using Microsoft.AspNetCore.Mvc;
using GreenCharge.Data;
using GreenCharge.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace GreenCharge.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. KAYIT OL (GET)
        public IActionResult Register()
        {
            return View();
        }

        // 1. KAYIT OL (POST)
        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            // Basit bir kayıt işlemi. Şifre şifreleme (Hashing) normalde şarttır ama ödev için plain-text bırakıyoruz.
            if (ModelState.IsValid)
            {
                // Varsayılan rol Member olsun. (Veya formdan seçtirebiliriz)
                // Şimdilik test için formdan ne gelirse onu kaydedelim.
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction("Login");
            }
            return View(user);
        }

        // 2. GİRİŞ YAP (GET)
        public IActionResult Login()
        {
            return View();
        }

        // 2. GİRİŞ YAP (POST)
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            // Kullanıcıyı veritabanında ara
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.Password == password);

            if (user != null)
            {
                // Kullanıcı bulundu, kimlik kartını (Claims) hazırlayalım
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim("FullName", user.FullName), // Özel veri
                    new Claim(ClaimTypes.Role, user.Role) // Rolü (Admin/Member) buraya yüklüyoruz
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // Çerezi oluştur ve giriş yap
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "E-posta veya şifre hatalı!";
            return View();
        }

        // 3. ÇIKIŞ YAP
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Çıkış yaptıktan sonra Anasayfaya veya Login'e at
            return RedirectToAction("Login", "Account");
        }
    }
}