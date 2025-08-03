using System.Threading.Tasks;
using UniversiteProjeYonetimSistemi.Models;

namespace UniversiteProjeYonetimSistemi.Services
{
    public interface IBildirimService
    {
        Task ProjeOlusturulduBildirimiGonder(Proje proje, string olusturanRol);
        Task ProjeYorumYapildiBildirimiGonder(ProjeYorum yorum, int projeId);
        Task DegerlendirmeYapildiBildirimiGonder(Degerlendirme degerlendirme);
        Task ProjeIlerlemesiDegistiBildirimiGonder(Proje proje);
        Task GorusmePlanlandiBildirimiGonder(DanismanlikGorusmesi gorusme);
        Task<int> OkunmamisBildirimSayisiniGetir(string kullaniciId, string rol);
        Task ProjeAsamasiTamamlandiBildirimiGonder(ProjeAsamasi projeAsamasi);
        Task GorusmeDurumuDegistiBildirimiGonder(DanismanlikGorusmesi gorusme);
    }
} 