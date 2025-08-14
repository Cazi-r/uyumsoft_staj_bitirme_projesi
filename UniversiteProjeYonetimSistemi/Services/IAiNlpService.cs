#nullable enable
using System;
using System.Threading.Tasks;

namespace UniversiteProjeYonetimSistemi.Services
{
	public record CreateProjectCommand(string Ad, string Kategori, string Aciklama);

	public interface IAiNlpService
	{
		Task<CreateProjectCommand?> ExtractCreateProjectAsync(string message);
		Task<DateTime?> ExtractDateTimeAsync(string message, DateTime? referenceTime = null);
	}
}


