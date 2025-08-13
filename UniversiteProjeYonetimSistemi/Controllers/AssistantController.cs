using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversiteProjeYonetimSistemi.Models;
using UniversiteProjeYonetimSistemi.Services;
using UniversiteProjeYonetimSistemi.Data;

namespace UniversiteProjeYonetimSistemi.Controllers
{
	[Route("assistant")]
	[Authorize]
	[IgnoreAntiforgeryToken]
	public class AssistantController : Controller
	{
		private readonly IProjeService _projeService;
		private readonly IRepository<ProjeKategori> _kategoriRepository;
		private readonly AuthService _authService;
		private readonly IAiNlpService _nlp;
		private readonly IOgrenciService _ogrenciService;
		private readonly IAkademisyenService _akademisyenService;
        private readonly ApplicationDbContext _db;
        private readonly IBildirimService _bildirimService;

		private static readonly ConcurrentDictionary<string, CreateProjectState> _sessions = new();
		private static readonly ConcurrentDictionary<string, CreateCategoryState> _categorySessions = new();
        private static readonly ConcurrentDictionary<string, CreateMeetingState> _meetingSessions = new();

		public AssistantController(
			IProjeService projeService,
			IRepository<ProjeKategori> kategoriRepository,
			AuthService authService,
			IAiNlpService nlp,
			IOgrenciService ogrenciService,
			IAkademisyenService akademisyenService,
            ApplicationDbContext db,
            IBildirimService bildirimService)
		{
			_projeService = projeService;
			_kategoriRepository = kategoriRepository;
			_authService = authService;
			_nlp = nlp;
			_ogrenciService = ogrenciService;
			_akademisyenService = akademisyenService;
            _db = db;
            _bildirimService = bildirimService;
		}

		public class ChatRequest { public string message { get; set; } }
		public class ChatResponse { public string reply { get; set; } public bool executed { get; set; } public object data { get; set; } }

		private enum CreateProjectStep { AskAd, AskAciklama, AskKategori, AskMentor, AskOgrenci, AskTeslimTarihi }
		private enum CreateCategoryStep { AskAd, AskAciklama, AskRenk }
        private enum CreateMeetingStep { AskProje, AskKarsiTaraf, AskBaslik, AskTarih, AskTip, AskNot }

		private class CreateProjectState
		{
			public string Ad { get; set; }
			public string Aciklama { get; set; }
			public string Kategori { get; set; }
			public string MentorAdSoyad { get; set; }
			public string OgrenciAdSoyad { get; set; }
			public DateTime? TeslimTarihi { get; set; }
			public CreateProjectStep? Pending { get; set; }
			public int? SelectedMentorId { get; set; }
			public int? SelectedOgrenciId { get; set; }
			public int[] MentorCandidateIds { get; set; }
			public int[] OgrenciCandidateIds { get; set; }
		}

		private class CreateCategoryState
		{
			public string Ad { get; set; }
			public string Aciklama { get; set; }
			public string Renk { get; set; }
			public CreateCategoryStep? Pending { get; set; }
		}

        private class CreateMeetingState
        {
            public int? ProjeId { get; set; }
            public string ProjeAd { get; set; }
            public int? OgrenciId { get; set; }
            public int? AkademisyenId { get; set; }
            public string KarsiTarafAdSoyad { get; set; }
            public string Baslik { get; set; }
            public DateTime? GorusmeTarihi { get; set; }
            public string GorusmeTipi { get; set; } // Online, Yüz Yüze
            public string Notlar { get; set; }
            public CreateMeetingStep? Pending { get; set; }
        }

		[HttpPost("chat")]
		public async Task<IActionResult> Chat([FromBody] ChatRequest req)
		{
			var text = req?.message ?? string.Empty;
			var userKey = User?.Identity?.Name ?? HttpContext.Connection.RemoteIpAddress?.ToString() ?? Guid.NewGuid().ToString();

			// İptal komutu
			if (Regex.IsMatch(text, "\\b(iptal|vazgeç|vazgec|cancel)\\b", RegexOptions.IgnoreCase))
			{
				_sessions.TryRemove(userKey, out _);
				_categorySessions.TryRemove(userKey, out _);
                _meetingSessions.TryRemove(userKey, out _);
				return Ok(new ChatResponse { reply = "Tamamdır, bu adımı iptal ettim. Hazır olduğunuzda yeniden başlayabiliriz.", executed = false });
			}

			// 1) Kategori olusturma akisi aktif mi?
			_categorySessions.TryGetValue(userKey, out var catState);
			if (catState != null)
			{
				// Yetki kontrolu
				if (!(User.IsInRole("Admin") || User.IsInRole("Akademisyen")))
				{
					_categorySessions.TryRemove(userKey, out _);
					return Ok(new ChatResponse { reply = "Bu işlem için yetkiniz yok gibi görünüyor. Proje oluşturma gibi işlemlerde yardımcı olabilirim.", executed = false });
				}

				if (catState.Pending.HasValue)
				{
					switch (catState.Pending.Value)
					{
						case CreateCategoryStep.AskAd:
							catState.Ad = text.Trim(' ', '"', '\'', '“', '”');
							break;
					case CreateCategoryStep.AskAciklama:
						if (!Regex.IsMatch(text, "^yok$", RegexOptions.IgnoreCase))
							catState.Aciklama = text.Trim();
						else
							catState.Aciklama = string.Empty; // yok denirse boş kabul et
						break;
					case CreateCategoryStep.AskRenk:
						if (Regex.IsMatch(text, "^yok$", RegexOptions.IgnoreCase))
						{
							// Varsayılan rengi seç
							catState.Renk = "#3B82F6";
						}
						else
						{
							var resolved = ResolveColor(text);
							if (string.IsNullOrEmpty(resolved))
								return Ok(new ChatResponse { reply = "Rengi '#RRGGBB' biçiminde ya da isim olarak yazabilirsiniz (ör. siyah, mavi). İsterseniz 'yok' diyebilirsiniz.", executed = false });
							catState.Renk = resolved;
						}
						break;
					}
					catState.Pending = null;
				}

				// Eksik alanlari sor
				if (string.IsNullOrWhiteSpace(catState.Ad))
				{
					catState.Pending = CreateCategoryStep.AskAd;
					return Ok(new ChatResponse { reply = "Yeni kategorimizin adı ne olsun?", executed = false });
				}
				if (catState.Aciklama == null)
				{
					catState.Pending = CreateCategoryStep.AskAciklama;
					return Ok(new ChatResponse { reply = "Kısa bir açıklama eklemek ister misiniz? (İsterseniz 'yok' yazabilirsiniz)", executed = false });
				}
				if (catState.Renk == null)
				{
					catState.Pending = CreateCategoryStep.AskRenk;
					return Ok(new ChatResponse { reply = "İsterseniz bir renk de belirleyelim (#RRGGBB veya isim: siyah, mavi...). (Örn: #3B82F6) Yoksa 'yok' yazabilirsiniz.", executed = false });
				}

				// Tum bilgiler var -> olustur
				var kategorilerAll = await _kategoriRepository.GetAllAsync();
				var duplicate = kategorilerAll.FirstOrDefault(k => k.Ad.Equals(catState.Ad, StringComparison.OrdinalIgnoreCase));
				if (duplicate != null)
				{
					_categorySessions.TryRemove(userKey, out _);
					var existsUrl = Url.Action("Details", "Kategori", new { id = duplicate.Id });
					return Ok(new ChatResponse { reply = $"Bu isimde bir kategori zaten var: '{duplicate.Ad}'. İsterseniz mevcut kategoriye gidebilirsiniz.", executed = true, data = new { categoryId = duplicate.Id, detailsUrl = existsUrl } });
				}

				var yeni = new ProjeKategori
				{
					Ad = catState.Ad.Trim(),
					Aciklama = catState.Aciklama?.Trim(),
					Renk = string.IsNullOrWhiteSpace(catState.Renk) ? "#3B82F6" : catState.Renk,
					CreatedAt = DateTime.Now,
					UpdatedAt = DateTime.Now
				};
				await _kategoriRepository.AddAsync(yeni);
				_categorySessions.TryRemove(userKey, out _);
				var urlCat = Url.Action("Details", "Kategori", new { id = yeni.Id });
				return Ok(new ChatResponse { reply = $"Harika! '{yeni.Ad}' kategorisini oluşturdum.", executed = true, data = new { categoryId = yeni.Id, detailsUrl = urlCat } });
			}

			// 2) Görüşme oluşturma akışı (öğrenci/akademisyen/admin)
            _meetingSessions.TryGetValue(userKey, out var meetState);
            if (meetState != null)
            {
                // Akışı yürüt
                var meetReply = await HandleMeetingFlow(userKey, text, meetState);
                if (meetReply != null) return meetReply;
            }

			// 3) Proje olusturma akisi

			// Mevcut oturum var mı?
			_sessions.TryGetValue(userKey, out var state);

			// Kategori oluşturma niyeti algıla
            if (catState == null && IsCreateCategoryIntent(text))
			{
				if (!(User.IsInRole("Admin") || User.IsInRole("Akademisyen")))
					return Ok(new ChatResponse { reply = "Kategori oluşturma için yetkiniz bulunmuyor. Proje oluşturma veya görüşme planlama konusunda yardımcı olabilirim.", executed = false });
				catState = new CreateCategoryState();
				_categorySessions[userKey] = catState;
				catState.Pending = CreateCategoryStep.AskAd;
				return Ok(new ChatResponse { reply = "Yeni kategorimizin adı ne olsun?", executed = false });
			}

            // 3-b) Meeting intent detection
            if (meetState == null && IsCreateMeetingIntent(text))
            {
                meetState = new CreateMeetingState();
                _meetingSessions[userKey] = meetState;
                meetState.Pending = CreateMeetingStep.AskProje;
                return Ok(new ChatResponse { reply = "Hangi proje için görüşme planlayalım? Proje adını paylaşır mısınız?", executed = false });
            }

            if (state == null)
			{
				// Yeni niyet algıla
				var llmCmd = await _nlp.ExtractCreateProjectAsync(text);
				var cmd = llmCmd ?? TryParseCreateProject(text);
				if (cmd == null)
				{
					// Niyet var ama isim yoksa akışı başlat ve isim iste
					if (IsCreateIntent(text))
					{
						state = new CreateProjectState();
						_sessions[userKey] = state;
					state.Pending = CreateProjectStep.AskAd;
					return Ok(new ChatResponse { reply = "Harika, yeni bir proje oluşturalım. Projenin adını paylaşır mısınız?", executed = false });
					}
					return Ok(new ChatResponse { reply = "Size yeni bir proje oluşturma, kategori belirleme ya da görüşme planlama konularında yardımcı olabilirim. Başlamak için örneğin 'Proje oluşturmak istiyorum' yazabilirsiniz.", executed = false });
				}
				state = new CreateProjectState
				{
					Ad = cmd.Ad,
					Aciklama = cmd.Aciklama,
					Kategori = cmd.Kategori
				};
				_sessions[userKey] = state;
			}
			else
			{
				// Beklenen soruya yanıt olarak alanı doldur
				if (state.Pending.HasValue)
				{
					switch (state.Pending.Value)
					{
						case CreateProjectStep.AskAd:
							state.Ad = text.Trim(' ', '"', '\'', '“', '”');
							break;
                    case CreateProjectStep.AskAciklama:
                        {
                            var trimmed = text.Trim();
                            if (string.IsNullOrWhiteSpace(trimmed) || Regex.IsMatch(trimmed, "^yok$", RegexOptions.IgnoreCase))
                            {
                                state.Aciklama = null; // zorunlu, boş kabul edilmez
                            }
                            else
                            {
                                state.Aciklama = trimmed;
                            }
                            break;
                        }
						case CreateProjectStep.AskKategori:
							if (!Regex.IsMatch(text, "^yok$", RegexOptions.IgnoreCase))
								state.Kategori = text.Trim(' ', '"', '\'', '“', '”');
							break;
						case CreateProjectStep.AskMentor:
							state.MentorAdSoyad = text.Trim();
							break;
						case CreateProjectStep.AskOgrenci:
							state.OgrenciAdSoyad = text.Trim();
							break;
						case CreateProjectStep.AskTeslimTarihi:
							if (!Regex.IsMatch(text, "^yok$", RegexOptions.IgnoreCase))
							{
								if (DateTime.TryParse(text, new System.Globalization.CultureInfo("tr-TR"), System.Globalization.DateTimeStyles.None, out var dt))
									state.TeslimTarihi = dt;
								else
									return Ok(new ChatResponse { reply = "Tarih formatını 'gg.aa.yyyy' gibi yazabilir misin? (ör. 15.09.2025) veya 'yok'", executed = false });
							}
							break;
					}
					state.Pending = null;
				}
				else
				{
					// Bekleme yoksa, serbest metinden alan kapmaya çalış
					var cmd = TryParseCreateProject(text);
					if (cmd != null)
					{
						state.Ad = state.Ad ?? cmd.Ad;
						state.Aciklama = state.Aciklama ?? cmd.Aciklama;
						state.Kategori = state.Kategori ?? cmd.Kategori;
					}
				}
			}

			// Sıradaki soru/eksikler
			if (string.IsNullOrWhiteSpace(state.Ad))
			{
				state.Pending = CreateProjectStep.AskAd;
				return Ok(new ChatResponse { reply = "Projenin adını paylaşır mısınız?", executed = false });
			}
            if (string.IsNullOrWhiteSpace(state.Aciklama))
			{
				state.Pending = CreateProjectStep.AskAciklama;
                return Ok(new ChatResponse { reply = "Açıklama zorunludur. Kısaca projenizi nasıl tanımlarsınız?", executed = false });
			}

			// Kategori zorunlu
			if (string.IsNullOrWhiteSpace(state.Kategori))
			{
				state.Pending = CreateProjectStep.AskKategori;
				var kategorilerList = (await _kategoriRepository.GetAllAsync()).Select(k => k.Ad).ToList();
				var listText = kategorilerList.Any() ? ("Mevcut kategoriler: " + string.Join(", ", kategorilerList)) : "Sistemde kayıtlı kategori görünmüyor.";
				return Ok(new ChatResponse { reply = $"Hangi kategoriye ekleyelim? {listText}\n(Kategori adını yazmanız yeterli.)", executed = false });
			}

			// Rol bazlı zorunlular
			if (User.IsInRole("Ogrenci") && state.SelectedMentorId == null && string.IsNullOrWhiteSpace(state.MentorAdSoyad))
			{
				state.Pending = CreateProjectStep.AskMentor;
				return Ok(new ChatResponse { reply = "Danışman olacak akademisyenin Ad Soyad bilgisini paylaşır mısınız?", executed = false });
			}
			if (User.IsInRole("Akademisyen") && state.SelectedOgrenciId == null && string.IsNullOrWhiteSpace(state.OgrenciAdSoyad))
			{
				state.Pending = CreateProjectStep.AskOgrenci;
				return Ok(new ChatResponse { reply = "Projeyi hangi öğrencimiz adına oluşturalım? Lütfen Ad Soyad paylaşın.", executed = false });
			}
			// Admin için hem öğrenci hem mentor bilgisi zorunlu: tarihi sormadan önce iste
			if (User.IsInRole("Admin"))
			{
				if (state.SelectedOgrenciId == null && string.IsNullOrWhiteSpace(state.OgrenciAdSoyad))
				{
					state.Pending = CreateProjectStep.AskOgrenci;
					return Ok(new ChatResponse { reply = "Projeyi hangi öğrencimiz adına oluşturalım? Lütfen Ad Soyad paylaşın.", executed = false });
				}
				if (state.SelectedMentorId == null && string.IsNullOrWhiteSpace(state.MentorAdSoyad))
				{
					state.Pending = CreateProjectStep.AskMentor;
					return Ok(new ChatResponse { reply = "Danışman olacak akademisyenin Ad Soyad bilgisini paylaşır mısınız?", executed = false });
				}
			}
			if (!state.TeslimTarihi.HasValue)
			{
				state.Pending = CreateProjectStep.AskTeslimTarihi;
				return Ok(new ChatResponse { reply = "Teslim tarihi var mı? Varsa gg.aa.yyyy şeklinde yazabilirsiniz; yoksa 'yok' da yazabilirsiniz.", executed = false });
			}

			// Tüm bilgiler alındı → doğrula ve oluştur
			ProjeKategori kategoriEntity = null;
			var kategoriler = await _kategoriRepository.GetAllAsync();
			kategoriEntity = FindBestMatchingKategori(kategoriler, state.Kategori);
			if (kategoriEntity == null)
			{
				state.Pending = CreateProjectStep.AskKategori;
				var listText = kategoriler.Any() ? ("Mevcut kategoriler: " + string.Join(", ", kategoriler.Select(k=>k.Ad))) : "Sistemde kayıtlı kategori görünmüyor.";
				return Ok(new ChatResponse { reply = $"Sanırım tam eşleşme bulamadım. Mevcut listeden bir isimle devam edebilir miyiz?\n{listText}", executed = false });
			}

			int? mentorId = state.SelectedMentorId;
			// Admin: mentor hiç verilmemişse önce iste, arama yapma
			if (User.IsInRole("Admin") && !mentorId.HasValue && string.IsNullOrWhiteSpace(state.MentorAdSoyad))
			{
				state.Pending = CreateProjectStep.AskMentor;
				return Ok(new ChatResponse { reply = "Danışman akademisyen bilgisi de gerekli. Lütfen Ad Soyad paylaşın.", executed = false });
			}
			if (!mentorId.HasValue && !string.IsNullOrWhiteSpace(state.MentorAdSoyad))
			{
				var mentor = await FindAkademisyenByFullNameAsync(state.MentorAdSoyad);
				if (mentor == null)
				{
				// Mentor bulunamadı uyar ve tekrar sor
				state.Pending = CreateProjectStep.AskMentor;
				return Ok(new ChatResponse { reply = $"Bu isimle bir akademisyen bulamadım. Ad ve Soyad'ı doğru sırayla tekrar yazar mısınız?", executed = false });
				}
				mentorId = mentor.Id;
				state.SelectedMentorId = mentorId;
			}

			int? ogrenciId = state.SelectedOgrenciId;
			if ((User.IsInRole("Akademisyen") || User.IsInRole("Admin")) && !ogrenciId.HasValue)
			{
				// Öğrenci adı hiç verilmediyse önce iste, arama yapma
				if (string.IsNullOrWhiteSpace(state.OgrenciAdSoyad))
				{
					state.Pending = CreateProjectStep.AskOgrenci;
					return Ok(new ChatResponse { reply = "Öğrenci bilgisini de ekleyelim. Lütfen Ad Soyad paylaşın.", executed = false });
				}
				var ogrenci = await FindOgrenciByFullNameAsync(state.OgrenciAdSoyad);
				if (ogrenci == null)
				{
				state.Pending = CreateProjectStep.AskOgrenci;
				return Ok(new ChatResponse { reply = $"Bu isimle bir öğrenci bulamadım. Adı ve Soyadıyla tekrar paylaşabilir misiniz?", executed = false });
				}
				ogrenciId = ogrenci.Id;
				state.SelectedOgrenciId = ogrenciId;
			}

			// Admin için her iki alan da zorunlu
			if (User.IsInRole("Admin"))
			{
				if (!ogrenciId.HasValue)
				{
					state.Pending = CreateProjectStep.AskOgrenci;
					return Ok(new ChatResponse { reply = "Öğrenci bilgisini ekleyelim. Lütfen Ad Soyad paylaşın.", executed = false });
				}
				if (!mentorId.HasValue)
				{
					state.Pending = CreateProjectStep.AskMentor;
					return Ok(new ChatResponse { reply = "Danışman akademisyen bilgisi de gerekli. Lütfen Ad Soyad paylaşın.", executed = false });
				}
			}

			var projeToCreate = new Proje
			{
				Ad = state.Ad.Trim(),
				Aciklama = state.Aciklama.Trim(),
				KategoriId = kategoriEntity?.Id,
				MentorId = mentorId,
				TeslimTarihi = state.TeslimTarihi,
				Status = "Beklemede"
			};

			if (User.IsInRole("Ogrenci"))
			{
				var ogrenci = await _authService.GetCurrentOgrenciAsync();
				if (ogrenci != null) projeToCreate.OgrenciId = ogrenci.Id;
			}
			else if (ogrenciId.HasValue)
			{
				projeToCreate.OgrenciId = ogrenciId.Value;
			}

			// Akademisyen projeyi açtıysa kendisini mentor olarak ata (eğer mentor belirtilmemişse)
			if (User.IsInRole("Akademisyen") && !mentorId.HasValue)
			{
				var me = await _authService.GetCurrentAkademisyenAsync();
				if (me != null) projeToCreate.MentorId = me.Id;
			}

			await _projeService.AddAsync(projeToCreate);
			_sessions.TryRemove(userKey, out _);
			var detailsUrl = Url.Action("Details", "Proje", new { id = projeToCreate.Id });
			var mentorDesc = projeToCreate.MentorId.HasValue ? "atanmış" : "atanmadı";
			var teslimText = projeToCreate.TeslimTarihi?.ToString("dd.MM.yyyy") ?? "-";
			var summary = $"Harika! '{projeToCreate.Ad}' projesini oluşturdum. Kategori: '{kategoriEntity?.Ad ?? "-"}', Mentor: {mentorDesc}, Teslim: {teslimText}. Detaylara buradan geçebilirsiniz.";
			return Ok(new ChatResponse { reply = summary, executed = true, data = new { projeId = projeToCreate.Id, detailsUrl } });
		}

		[HttpPost("reset")]
		public IActionResult Reset()
		{
			var userKey = User?.Identity?.Name ?? HttpContext.Connection.RemoteIpAddress?.ToString() ?? Guid.NewGuid().ToString();
			_sessions.TryRemove(userKey, out _);
			_categorySessions.TryRemove(userKey, out _);
			_meetingSessions.TryRemove(userKey, out _);
			return Ok();
		}

		private CreateProjectCommand TryParseCreateProject(string text)
		{
			if (string.IsNullOrWhiteSpace(text)) return null;
			if (!IsCreateIntent(text)) return null;

			string ad = null, kategori = null, aciklama = null;
			var mAd = Regex.Match(text, "ad[ıi]?\\s+(?<name>.+?)\\s+olsun", RegexOptions.IgnoreCase);
			if (!mAd.Success)
				mAd = Regex.Match(text, "(?:ad[ıi]?|ismi)\\s*[:\\-]?\\s*['\\\"](?<name>[^'\\\"]+)['\\\"]", RegexOptions.IgnoreCase);
			if (mAd.Success) ad = mAd.Groups["name"].Value.Trim(' ', '"', '\'', '“', '”');

			var mKat = Regex.Match(text, "kategori(?:si|de|sinde|)\\s+(?<cat>.+?)(?:\\s+olsun|[,\\.]|$)", RegexOptions.IgnoreCase);
			if (mKat.Success) kategori = mKat.Groups["cat"].Value.Trim(' ', '"', '\'', '“', '”');

			var mAcik = Regex.Match(text, "açıklama\\s*[:\\-]\\s*(?<a>.+)$", RegexOptions.IgnoreCase);
			if (mAcik.Success) aciklama = mAcik.Groups["a"].Value.Trim();

			return string.IsNullOrWhiteSpace(ad) ? null : new CreateProjectCommand(ad, kategori ?? string.Empty, aciklama ?? string.Empty);
		}

		private bool IsCreateIntent(string text)
		{
			var l = text.ToLower(new System.Globalization.CultureInfo("tr-TR"));
			return l.Contains("proje") && (l.Contains("aç") || l.Contains("ac") || l.Contains("oluştur") || l.Contains("olustur") || l.Contains("ekle") || l.Contains("kur") || l.Contains("olusturmak") || l.Contains("oluşturmak"));
		}

		private bool IsCreateCategoryIntent(string text)
		{
			var l = text.ToLower(new System.Globalization.CultureInfo("tr-TR"));
			return (l.Contains("kategori") || l.Contains("kategorİ")) && (l.Contains("oluştur") || l.Contains("olustur") || l.Contains("ekle") || l.Contains("kur") || l.Contains("yeni"));
		}

        private bool IsCreateMeetingIntent(string text)
        {
            var l = text.ToLower(new System.Globalization.CultureInfo("tr-TR"));
            return (l.Contains("görüşme") || l.Contains("gorusme") || l.Contains("randevu")) &&
                   (l.Contains("planla") || l.Contains("olustur") || l.Contains("oluştur") || l.Contains("talep") || l.Contains("ekle"));
        }

		private async Task<ProjeKategori> EnsureKategoriAsync(string ad)
		{
			if (string.IsNullOrWhiteSpace(ad)) return null;
			var kategoriler = await _kategoriRepository.GetAllAsync();
			var mevcut = kategoriler.FirstOrDefault(k => string.Equals(k.Ad, ad, StringComparison.OrdinalIgnoreCase));
			if (mevcut != null) return mevcut;
			if (!(User.IsInRole("Admin") || User.IsInRole("Akademisyen"))) return null;
			var yeni = new ProjeKategori { Ad = ad.Trim(), CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now };
			await _kategoriRepository.AddAsync(yeni);
			return yeni;
		}

		private async Task<Akademisyen> FindAkademisyenByFullNameAsync(string adSoyad)
		{
			if (string.IsNullOrWhiteSpace(adSoyad)) return null;
			var parts = adSoyad.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length < 2) return null;
			var ad = parts[0];
			var soyad = parts[^1];
			var list = await _akademisyenService.GetAllAsync();
			return list.FirstOrDefault(a => a.Ad.Equals(ad, StringComparison.OrdinalIgnoreCase) && a.Soyad.Equals(soyad, StringComparison.OrdinalIgnoreCase));
		}

		private ProjeKategori FindBestMatchingKategori(System.Collections.Generic.IEnumerable<ProjeKategori> kategoriler, string input)
		{
			if (kategoriler == null) return null;
			if (string.IsNullOrWhiteSpace(input)) return null;
			var normInput = Normalize(input);
			ProjeKategori best = null;
			int bestDistance = int.MaxValue;

			foreach (var k in kategoriler)
			{
				var name = k?.Ad;
				if (string.IsNullOrWhiteSpace(name)) continue;
				var normName = Normalize(name);
				// Exact case-insensitive
				if (normName == normInput) return k;
			}

			// StartsWith priority
			var starts = kategoriler.Where(k => !string.IsNullOrWhiteSpace(k.Ad) && Normalize(k.Ad).StartsWith(normInput)).ToList();
			if (starts.Count == 1) return starts[0];
			if (starts.Count > 1)
			{
				// choose minimal distance
				foreach (var k in starts)
				{
					var d = Levenshtein(Normalize(k.Ad), normInput);
					if (d < bestDistance) { best = k; bestDistance = d; }
				}
				return best;
			}

			// Contains fallback
			var contains = kategoriler.Where(k => !string.IsNullOrWhiteSpace(k.Ad) && Normalize(k.Ad).Contains(normInput)).ToList();
			if (contains.Count == 1) return contains[0];
			if (contains.Count > 1)
			{
				foreach (var k in contains)
				{
					var d = Levenshtein(Normalize(k.Ad), normInput);
					if (d < bestDistance) { best = k; bestDistance = d; }
				}
				return best;
			}

			// Global best by distance
			foreach (var k in kategoriler)
			{
				var d = Levenshtein(Normalize(k.Ad), normInput);
				if (d < bestDistance) { best = k; bestDistance = d; }
			}
			return best;
		}

		private string Normalize(string s)
		{
			if (string.IsNullOrWhiteSpace(s)) return string.Empty;
			s = s.Trim().ToLower(new System.Globalization.CultureInfo("tr-TR"));
			s = System.Text.RegularExpressions.Regex.Replace(s, "\\s+", " ");
			return s;
		}

		private int Levenshtein(string a, string b)
		{
			if (a == null) a = string.Empty;
			if (b == null) b = string.Empty;
			int n = a.Length, m = b.Length;
			var dp = new int[n + 1, m + 1];
			for (int i = 0; i <= n; i++) dp[i, 0] = i;
			for (int j = 0; j <= m; j++) dp[0, j] = j;
			for (int i = 1; i <= n; i++)
			{
				for (int j = 1; j <= m; j++)
				{
					int cost = a[i - 1] == b[j - 1] ? 0 : 1;
					dp[i, j] = Math.Min(Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1), dp[i - 1, j - 1] + cost);
				}
			}
			return dp[n, m];
		}

		private string ResolveColor(string input)
		{
			if (string.IsNullOrWhiteSpace(input)) return null;
			var v = input.Trim().ToLower(new System.Globalization.CultureInfo("tr-TR"));
			// Hex ise direkt kabul
			if (Regex.IsMatch(v, "^#[0-9a-f]{6}$")) return v;
			// Renk isimleri haritası (temel)
			switch (v)
			{
				case "siyah": return "#000000";
				case "beyaz": return "#FFFFFF";
				case "kirmizi":
				case "kırmızı": return "#FF0000";
				case "yesil":
				case "yeşil": return "#00FF00";
				case "mavi": return "#0000FF";
				case "turuncu": return "#FFA500";
				case "mor": return "#800080";
				case "pembe": return "#FFC0CB";
				case "gri": return "#808080";
				case "lacivert": return "#000080";
				case "kahverengi": return "#8B4513";
				case "sarI":
				case "sarı": return "#FFFF00";
				default:
					// Bilinmeyen ise null dön
					return null;
			}
		}
        private async Task<IActionResult> HandleMeetingFlow(string userKey, string text, CreateMeetingState state)
        {
            // Role-specific available projects
            var roleIsOgrenci = User.IsInRole("Ogrenci");
            var roleIsAkademisyen = User.IsInRole("Akademisyen");
            var roleIsAdmin = User.IsInRole("Admin");

            // Capture step answers
            if (state.Pending.HasValue)
            {
                switch (state.Pending.Value)
                {
                    case CreateMeetingStep.AskProje:
                    {
                        var inputText = (text ?? string.Empty).Trim();
                        // Cok kisa girdilerde (3 karakterden az) otomatik eslestirme yapmayalim; liste sunalim
                        if (string.IsNullOrWhiteSpace(inputText) || inputText.Length < 3)
                        {
                            var names = await GetAvailableProjectNamesForPromptAsync();
                            var listText = names.Count > 0
                                ? "Mevcut projeleriniz: " + string.Join(", ", names.Count > 10 ? names.GetRange(0, 10) : names) + (names.Count > 10 ? " ve digerleri..." : string.Empty) + "."
                                : "Su anda sizinle iliskili listelenebilir proje bulunmuyor.";
                            return Ok(new ChatResponse { reply = listText + " Lutfen proje adindan en az 3 harf yazin ya da tam adini girin.", executed = false });
                        }

                        var proj = await FindProjectByNameForCurrentUserAsync(inputText);
                        if (proj == null)
                        {
                            var names = await GetAvailableProjectNamesForPromptAsync();
                            var listText = names.Count > 0
                                ? " Mevcut projeleriniz: " + string.Join(", ", names.Count > 10 ? names.GetRange(0, 10) : names) + (names.Count > 10 ? " ve digerleri..." : string.Empty) + "."
                                : " Su anda sizinle iliskili listelenebilir proje bulunmuyor.";
                            return Ok(new ChatResponse { reply = "Bu ada yakin bir proje bulamadim." + listText + " Proje adini yazmaniz yeterli.", executed = false });
                        }
                        state.ProjeId = proj.Id;
                        state.ProjeAd = proj.Ad;
                        if (proj.OgrenciId.HasValue) state.OgrenciId = proj.OgrenciId.Value;
                        if (proj.MentorId.HasValue) state.AkademisyenId = proj.MentorId.Value;
                        state.Pending = null;
                        break;
                    }
                    case CreateMeetingStep.AskKarsiTaraf:
                    {
                        // When missing participant for admin
                        var parts = text.Trim();
                        // Try first as Student, then as Akademisyen
                        if (!state.OgrenciId.HasValue)
                        {
                            var ogr = await FindOgrenciByFullNameAsync(parts);
                            if (ogr != null)
                            {
                                state.OgrenciId = ogr.Id;
                                state.Pending = null;
                                break;
                            }
                        }
                        if (!state.AkademisyenId.HasValue)
                        {
                            var aka = await FindAkademisyenByFullNameAsync(parts);
                            if (aka != null)
                            {
                                state.AkademisyenId = aka.Id;
                                state.Pending = null;
                                break;
                            }
                        }
                        return Ok(new ChatResponse { reply = "İsmi eşleştiremedim. Lütfen Ad Soyad şeklinde tekrar paylaşır mısınız?", executed = false });
                    }
                    case CreateMeetingStep.AskBaslik:
                    {
                        var t = text.Trim();
                        if (string.IsNullOrWhiteSpace(t))
                            return Ok(new ChatResponse { reply = "Görüşme için kısa bir başlık yazar mısınız?", executed = false });
                        state.Baslik = t;
                        state.Pending = null;
                        break;
                    }
                    case CreateMeetingStep.AskTarih:
                    {
                        // 1) LLM ile esnek tarih/saat cikarimi
                        var dtFromNlp = await _nlp.ExtractDateTimeAsync(text, DateTime.Now);
                        if (dtFromNlp.HasValue)
                        {
                            state.GorusmeTarihi = dtFromNlp.Value;
                            state.Pending = null;
                            break;
                        }

                        // 2) Standart TryParse yedek
                        if (DateTime.TryParse(text, new System.Globalization.CultureInfo("tr-TR"), System.Globalization.DateTimeStyles.None, out var dt))
                        {
                            state.GorusmeTarihi = dt;
                            state.Pending = null;
                            break;
                        }

                        // 3) Orneklerle tekrar iste
                        return Ok(new ChatResponse { reply = "Tarihi tam anlayamadim. Su orneklerden birini yazabilir misiniz? (21.08.2026 14:30, 'yarin 14:00', '28 agustos 14:00')", executed = false });
                    }
                    case CreateMeetingStep.AskTip:
                    {
                        var l = text.Trim().ToLower(new System.Globalization.CultureInfo("tr-TR"));
                        if (l.Contains("online")) state.GorusmeTipi = "Online";
                        else if (l.Contains("yuz") || l.Contains("yüz") || l.Contains("yuz yuze") || l.Contains("yüz yüze") || l.Contains("ofis") || l.Contains("fizik")) state.GorusmeTipi = "Yüz Yüze";
                        else if (Regex.IsMatch(l, "^yok$", RegexOptions.IgnoreCase)) state.GorusmeTipi = "Online";
                        else return Ok(new ChatResponse { reply = "Görüşme tipi 'Online' mı 'Yüz Yüze' mi olsun?", executed = false });
                        state.Pending = null;
                        break;
                    }
                    case CreateMeetingStep.AskNot:
                    {
                        var t = text.Trim();
                        state.Notlar = Regex.IsMatch(t, "^yok$", RegexOptions.IgnoreCase) ? string.Empty : t;
                        state.Pending = null;
                        break;
                    }
                }
            }

            // Ask missing info in order: Proje -> Baslik -> Tarih -> Tip -> Not
            if (!state.ProjeId.HasValue)
            {
                state.Pending = CreateMeetingStep.AskProje;
                var names = await GetAvailableProjectNamesForPromptAsync();
                var listText = names.Count > 0
                    ? "Mevcut projeleriniz: " + string.Join(", ", names.Count > 10 ? names.GetRange(0, 10) : names) + (names.Count > 10 ? " ve digerleri..." : string.Empty)
                    : "Henuz listelenebilir projeniz yok.";
                var prompt = "Hangi proje icin gorusme planlayalim? " + listText + " (Proje adini yazmaniz yeterli.)";
                return Ok(new ChatResponse { reply = prompt, executed = false });
            }

            if (string.IsNullOrWhiteSpace(state.Baslik))
            {
                state.Pending = CreateMeetingStep.AskBaslik;
                return Ok(new ChatResponse { reply = "Görüşme için kısa bir başlık yazar mısınız?", executed = false });
            }

            if (!state.GorusmeTarihi.HasValue)
            {
                state.Pending = CreateMeetingStep.AskTarih;
                return Ok(new ChatResponse { reply = "Görüşme tarihi ve saati nedir? Or: 21.08.2026 14:30, 'yarin 14:00', '28 agustos 14:00'", executed = false });
            }

            if (string.IsNullOrWhiteSpace(state.GorusmeTipi))
            {
                state.Pending = CreateMeetingStep.AskTip;
                return Ok(new ChatResponse { reply = "Görüşme tipi 'Online' mı 'Yüz Yüze' mi olsun?", executed = false });
            }

            if (state.Notlar == null)
            {
                state.Pending = CreateMeetingStep.AskNot;
                return Ok(new ChatResponse { reply = "İsterseniz not ekleyebilirsiniz. (Yoksa 'yok' yazabilirsiniz)", executed = false });
            }

            // Participants per role
            if (roleIsOgrenci)
            {
                var ogr = await _authService.GetCurrentOgrenciAsync();
                if (ogr == null) return Ok(new ChatResponse { reply = "Öğrenci bilginize erişemedim.", executed = false });
                state.OgrenciId = ogr.Id;
                if (!state.AkademisyenId.HasValue)
                {
                    var proj = await _db.Projeler.FindAsync(state.ProjeId.Value);
                    if (proj?.MentorId == null) return Ok(new ChatResponse { reply = "Projenize atanmış bir danışman bulunamadı. Lütfen önce projeye danışman atayın.", executed = false });
                    state.AkademisyenId = proj.MentorId.Value;
                }
            }
            else if (roleIsAkademisyen)
            {
                var aka = await _authService.GetCurrentAkademisyenAsync();
                if (aka == null) return Ok(new ChatResponse { reply = "Akademisyen bilginize erişemedim.", executed = false });
                state.AkademisyenId = aka.Id;
                if (!state.OgrenciId.HasValue)
                {
                    var proj = await _db.Projeler.FindAsync(state.ProjeId.Value);
                    if (proj?.OgrenciId == null) return Ok(new ChatResponse { reply = "Projeye atanmış bir öğrenci bulunamadı.", executed = false });
                    state.OgrenciId = proj.OgrenciId.Value;
                }
            }
            else if (roleIsAdmin)
            {
                // Admin: proje seçiliyse katılımcıları tamamlamaya çalış
                var proj = await _db.Projeler.FindAsync(state.ProjeId.Value);
                if (!state.AkademisyenId.HasValue && proj?.MentorId != null) state.AkademisyenId = proj.MentorId.Value;
                if (!state.OgrenciId.HasValue && proj?.OgrenciId != null) state.OgrenciId = proj.OgrenciId.Value;

                if (!state.OgrenciId.HasValue || !state.AkademisyenId.HasValue)
                {
                    state.Pending = CreateMeetingStep.AskKarsiTaraf;
                    return Ok(new ChatResponse { reply = "Görüşme katılımcılarından eksik olanın Ad Soyad bilgisini yazar mısınız?", executed = false });
                }
            }

            // Create meeting
            var meeting = new DanismanlikGorusmesi
            {
                ProjeId = state.ProjeId.Value,
                OgrenciId = state.OgrenciId.Value,
                AkademisyenId = state.AkademisyenId.Value,
                Baslik = state.Baslik,
                GorusmeTarihi = state.GorusmeTarihi.Value,
                GorusmeTipi = state.GorusmeTipi,
                Notlar = state.Notlar,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            if (roleIsOgrenci)
            {
                meeting.TalepEden = "Ogrenci";
                meeting.Durum = GorusmeDurumu.HocaOnayiBekliyor;
                meeting.SonGuncelleyenRol = "Ogrenci";
            }
            else if (roleIsAkademisyen)
            {
                meeting.TalepEden = "Akademisyen";
                meeting.Durum = GorusmeDurumu.OgrenciOnayiBekliyor;
                meeting.SonGuncelleyenRol = "Akademisyen";
            }
            else
            {
                meeting.TalepEden = "Admin";
                meeting.Durum = GorusmeDurumu.Onaylandi;
                meeting.SonGuncelleyenRol = "Admin";
            }

            meeting.GuncelleZamanDurumu();
            _db.DanismanlikGorusmeleri.Add(meeting);
            await _db.SaveChangesAsync();

            // Notify
            await _bildirimService.GorusmePlanlandiBildirimiGonder(meeting);

            _meetingSessions.TryRemove(userKey, out _);
            var detailsUrl = Url.Action("Details", "DanismanlikGorusmesi", new { id = meeting.Id });
            var reply = $"Görüşme talebini oluşturdum. Başlık: '{meeting.Baslik}', Tarih: {meeting.GorusmeTarihi:dd.MM.yyyy HH:mm}, Tip: {meeting.GorusmeTipi}. Detaylara buradan geçebilirsiniz.";
            return Ok(new ChatResponse { reply = reply, executed = true, data = new { meetingId = meeting.Id, detailsUrl } });
        }

        private async Task<Proje> FindProjectByNameForCurrentUserAsync(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            var normInput = Normalize(input);
            IQueryable<Proje> q = _db.Projeler;
            if (User.IsInRole("Ogrenci"))
            {
                var ogr = await _authService.GetCurrentOgrenciAsync();
                if (ogr == null) return null;
                q = q.Where(p => p.OgrenciId == ogr.Id);
            }
            else if (User.IsInRole("Akademisyen"))
            {
                var aka = await _authService.GetCurrentAkademisyenAsync();
                if (aka == null) return null;
                q = q.Where(p => p.MentorId == aka.Id);
            }
            var list = await Task.FromResult(q.ToList());
            Proje best = null;
            int bestDist = int.MaxValue;
            foreach (var p in list)
            {
                var n = Normalize(p.Ad);
                if (n == normInput) return p;
            }
            var starts = list.Where(p => Normalize(p.Ad).StartsWith(normInput)).ToList();
            if (starts.Count == 1) return starts[0];
            if (starts.Count > 1)
            {
                foreach (var p in starts)
                {
                    var d = Levenshtein(Normalize(p.Ad), normInput);
                    if (d < bestDist) { best = p; bestDist = d; }
                }
                return best;
            }
            var contains = list.Where(p => Normalize(p.Ad).Contains(normInput)).ToList();
            if (contains.Count == 1) return contains[0];
            if (contains.Count > 1)
            {
                foreach (var p in contains)
                {
                    var d = Levenshtein(Normalize(p.Ad), normInput);
                    if (d < bestDist) { best = p; bestDist = d; }
                }
                return best;
            }
            foreach (var p in list)
            {
                var d = Levenshtein(Normalize(p.Ad), normInput);
                if (d < bestDist) { best = p; bestDist = d; }
            }
            return best;
        }

		private async Task<List<string>> GetAvailableProjectNamesForPromptAsync()
		{
			IQueryable<Proje> q = _db.Projeler;
			if (User.IsInRole("Ogrenci"))
			{
				var ogr = await _authService.GetCurrentOgrenciAsync();
				if (ogr != null) q = q.Where(p => p.OgrenciId == ogr.Id);
			}
			else if (User.IsInRole("Akademisyen"))
			{
				var aka = await _authService.GetCurrentAkademisyenAsync();
				if (aka != null) q = q.Where(p => p.MentorId == aka.Id);
			}
			var list = await Task.FromResult(q.Select(p => p.Ad).ToList());
			return list;
		}

		private async Task<Ogrenci> FindOgrenciByFullNameAsync(string adSoyad)
		{
			if (string.IsNullOrWhiteSpace(adSoyad)) return null;
			var parts = adSoyad.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length < 2) return null;
			var ad = parts[0];
			var soyad = parts[^1];
			var list = await _ogrenciService.GetAllAsync();
			return list.FirstOrDefault(o => o.Ad.Equals(ad, StringComparison.OrdinalIgnoreCase) && o.Soyad.Equals(soyad, StringComparison.OrdinalIgnoreCase));
		}
	}
}


