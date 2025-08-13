using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace UniversiteProjeYonetimSistemi.Services
{
	public class GeminiNlpService : IAiNlpService
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly string _apiKey;
		private readonly string _model;

		public GeminiNlpService(IHttpClientFactory httpClientFactory, IConfiguration config)
		{
			_httpClientFactory = httpClientFactory;
			_apiKey = config["Gemini:ApiKey"];
			_model = config["Gemini:Model"] ?? "gemini-1.5-flash";
		}

		public async Task<CreateProjectCommand?> ExtractCreateProjectAsync(string message)
		{
			if (string.IsNullOrWhiteSpace(_apiKey)) return null;

			var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";
			var http = _httpClientFactory.CreateClient();

			var payload = new
			{
				contents = new[]
				{
					new
					{
						role = "user",
						parts = new[] { new { text = message } }
					}
				},
				tools = new[]
				{
					new
					{
						functionDeclarations = new[]
						{
							new
							{
								name = "create_project",
								description = "Create a project in the system",
								parameters = new
								{
									type = "OBJECT",
									properties = new
									{
										ad = new { type = "STRING", description = "Project name" },
										kategori = new { type = "STRING", description = "Category name" },
										aciklama = new { type = "STRING", description = "Description" }
									},
									required = new[] { "ad" }
								}
							}
						}
					}
				}
			};

			var req = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
			var res = await http.PostAsync(url, req);
			if (!res.IsSuccessStatusCode) return null;

			using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
			if (!doc.RootElement.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
				return null;

			var parts = candidates[0].GetProperty("content").GetProperty("parts");
			foreach (var part in parts.EnumerateArray())
			{
				if (part.TryGetProperty("functionCall", out var fc))
				{
					var name = fc.GetProperty("name").GetString();
					if (name == "create_project")
					{
						var args = fc.GetProperty("args");
						var ad = args.TryGetProperty("ad", out var adEl) ? adEl.GetString() : null;
						var kategori = args.TryGetProperty("kategori", out var kEl) ? kEl.GetString() : null;
						var aciklama = args.TryGetProperty("aciklama", out var aEl) ? aEl.GetString() : null;
						if (!string.IsNullOrWhiteSpace(ad))
							return new CreateProjectCommand(ad!.Trim(), kategori?.Trim() ?? string.Empty, aciklama?.Trim() ?? string.Empty);
					}
				}
			}
			return null;
		}
	}
}


