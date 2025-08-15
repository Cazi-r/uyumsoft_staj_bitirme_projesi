using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using UniversiteProjeYonetimSistemi.Services;
using UniversiteProjeYonetimSistemi.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

namespace UniversiteProjeYonetimSistemi.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        // Login formunu gosterir. returnUrl varsa, basarili giristen sonra oraya geri yonlendirir.
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // Login POST: Model dogruysa AuthService ile giris yapar, cookie olusturulur.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                if (await _authService.LoginAsync(model.Email, model.Password))
                {
                    return RedirectToLocal(returnUrl);
                }

                ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi.");
            }

            return View(model);
        }

        // Cikis yapar: cookie'yi/oturumu siler. [Authorize] ile sadece girisli kullanici erisir.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }

        // Register formunu gosterir. Roller listesi (Ogrenci/Akademisyen/Admin) ViewBag ile saglanir.
        public IActionResult Register()
        {
            var model = new RegisterViewModel();
            ViewBag.Roller = new List<string> { "Ogrenci", "Akademisyen", "Admin" };
            return View(model);
        }

        // Register POST: Model dogruysa kullaniciyi olusturur (rolune gore bagli tabloyu ekler).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            ViewBag.Roller = new List<string> { "Ogrenci", "Akademisyen", "Admin" };

            if (ModelState.IsValid)
            {
                var result = await _authService.RegisterAsync(model);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Kayıt işlemi başarılı. Şimdi giriş yapabilirsiniz.";
                    return RedirectToAction(nameof(Login));
                }

                ModelState.AddModelError(string.Empty, result.Message);
            }

            return View(model);
        }

        // Yetki yetersizliginde cookie ayarlarindaki AccessDeniedPath ile yonlendirilen sayfayi dondurur.
        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
} 