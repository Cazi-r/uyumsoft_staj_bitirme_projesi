using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using UniversiteProjeYonetimSistemi.Services;

namespace UniversiteProjeYonetimSistemi.Middleware
{
    public class ZamanDurumuUpdateMiddleware
    {
        private readonly RequestDelegate _next;
        private static DateTime _lastUpdateTime = DateTime.MinValue;

        public ZamanDurumuUpdateMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ZamanDurumuService zamanDurumuService)
        {
            // Günde bir kez ZamanDurumu değerlerini güncelle
            if (DateTime.Now.Date > _lastUpdateTime.Date)
            {
                await zamanDurumuService.UpdateAllZamanDurumuAsync();
                _lastUpdateTime = DateTime.Now;
            }

            // Middleware zincirinde bir sonraki adıma geç
            await _next(context);
        }
    }

    // Extension metodu ekleyelim
    public static class ZamanDurumuUpdateMiddlewareExtensions
    {
        public static IApplicationBuilder UseZamanDurumuUpdate(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ZamanDurumuUpdateMiddleware>();
        }
    }
} 