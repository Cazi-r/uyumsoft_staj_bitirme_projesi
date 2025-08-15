using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using UniversiteProjeYonetimSistemi.Services;
using System.Security.Principal;

namespace UniversiteProjeYonetimSistemi.ViewComponents
{
    public class BildirimSayaciViewComponent : ViewComponent
    {
        private readonly IBildirimService _bildirimService;

        public BildirimSayaciViewComponent(IBildirimService bildirimService)
        {
            _bildirimService = bildirimService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!User.Identity.IsAuthenticated)
                return Content("");

            var claimsUser = User as ClaimsPrincipal;
            var userId = claimsUser?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = claimsUser?.FindFirst(ClaimTypes.Role)?.Value;
            
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userRole) || userRole == "Admin")
                return Content("");

            var bildirimSayisi = await _bildirimService.OkunmamisBildirimSayisiniGetir(userId, userRole);

            return View(bildirimSayisi);
        }
    }
} 