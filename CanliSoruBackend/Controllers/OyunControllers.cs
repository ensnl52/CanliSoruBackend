using CanliSoruBackend.Data;
using CanliSoruBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CanliSoruBackend.Controllers
{
    [Route("api/Oyun")]
    [ApiController]
    [Authorize]
    public class OyunController : ControllerBase
      
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OyunController(
      AppDbContext context,
      UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }       

        [HttpPost("katil")]
        public async Task<IActionResult> OyunaKatil(
    string odaKodu)
        {
            var kullaniciId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(kullaniciId))
            {
                return Unauthorized();
            }

            var oyun = await _context.Oyunlar
                .FirstOrDefaultAsync(x =>
                    x.OdaKodu == odaKodu);

            if (oyun == null)
            {
                return NotFound("Oda bulunamadı.");
            }

            var zatenKatildi =
                await _context.OyunOyunculari
                    .AnyAsync(x =>
                        x.OyunId == oyun.Id &&
                        x.KullaniciId == kullaniciId);

            if (zatenKatildi)
            {
                return BadRequest(
                    "Bu oyuncu zaten oyunda."
                );
            }

            var oyuncu = new OyunOyuncu
            {
                OyunId = oyun.Id,
                KullaniciId = kullaniciId,
                Puan = 0
            };

            _context.OyunOyunculari.Add(oyuncu);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mesaj = "Oyuna başarıyla katıldınız.",
                oyunId = oyun.Id,
                odaKodu = oyun.OdaKodu
            });
        }
        [HttpGet("{oyunId}/oyuncular")]
        public async Task<IActionResult> OyunculariGetir(int oyunId)
        {
            var kullaniciId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(kullaniciId))
            {
                return Unauthorized();
            }

            var oyun = await _context.Oyunlar
                .FirstOrDefaultAsync(x => x.Id == oyunId);

            if (oyun == null)
            {
                return NotFound("Oyun bulunamadı.");
            }

            var oyuncular = await _context.OyunOyunculari
                .Where(x => x.OyunId == oyunId)
                .ToListAsync();

            var sonuc = new List<object>();

            foreach (var oyuncu in oyuncular)
            {
                var kullanici =
                    await _userManager.FindByIdAsync(
                        oyuncu.KullaniciId
                    );

                sonuc.Add(new
                {
                    kullaniciId = oyuncu.KullaniciId,
                    kullaniciAdi = kullanici?.UserName ?? "Bilinmeyen Kullanıcı",
                    puan = oyuncu.Puan
                });
            }

            return Ok(sonuc);
        
        }

        [HttpPost("baslat")]
        public async Task<IActionResult> OyunBaslat()
        {
            var kullaniciId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(kullaniciId))
            {
                return Unauthorized();
            }

            var sorular = await _context.Sorular
                .OrderBy(x => Guid.NewGuid())
                .Take(20)
                .ToListAsync();

            if (sorular.Count < 20)
            {
                return BadRequest(
                    $"Oyunu başlatmak için en az 20 soru gerekli. Şu anda {sorular.Count} soru var."
                );
            }

            var odaKodu = Guid.NewGuid()
                .ToString("N")
                .Substring(0, 6)
                .ToUpper();

            var oyun = new Oyun
            {
                OdaKodu = odaKodu,
                Tarih = DateTime.UtcNow,
                BaslangicZamani = DateTime.UtcNow
            };

            _context.Oyunlar.Add(oyun);

            await _context.SaveChangesAsync();

            var oyuncu = new OyunOyuncu
            {
                OyunId = oyun.Id,
                KullaniciId = kullaniciId,
                Puan = 0
            };

            _context.OyunOyunculari.Add(oyuncu);

            for (int i = 0; i < sorular.Count; i++)
            {
                _context.OyunSorulari.Add(
                    new OyunSoru
                    {
                        OyunId = oyun.Id,
                        SoruId = sorular[i].Id,
                        Sira = i + 1
                    }
                );
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                oyunId = oyun.Id,
                odaKodu = oyun.OdaKodu,
                sorular = sorular
            });
        }

        [HttpGet("{oyunId}/sorular")]
        public async Task<IActionResult> OyunSorulariGetir(int oyunId)
        {
            var kullaniciId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(kullaniciId))
            {
                return Unauthorized();
            }

            var oyun = await _context.Oyunlar
                .FirstOrDefaultAsync(x =>
                    x.Id == oyunId &&
                    x.KullaniciId == kullaniciId);

            if (oyun == null)
            {
                return NotFound("Oyun bulunamadı.");
            }

            var sorular = await _context.OyunSorulari
                .Where(x => x.OyunId == oyunId)
                .Include(x => x.Soru)
                .OrderBy(x => x.Sira)
                .Select(x => x.Soru)
                .ToListAsync();

            return Ok(sorular);
        }

        [HttpPost("{oyunId}/soru/{soruId}/baslat")]
        public async Task<IActionResult> SoruBaslat(
            int oyunId,
            int soruId)
        {
            var kullaniciId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(kullaniciId))
            {
                return Unauthorized();
            }

            var oyun = await _context.Oyunlar
                .FirstOrDefaultAsync(x =>
                    x.Id == oyunId &&
                    x.KullaniciId == kullaniciId);

            if (oyun == null)
            {
                return NotFound("Oyun bulunamadı.");
            }

            var oyunSoru = await _context.OyunSorulari
                .FirstOrDefaultAsync(x =>
                    x.OyunId == oyunId &&
                    x.SoruId == soruId);

            if (oyunSoru == null)
            {
                return NotFound(
                    "Bu soru bu oyuna ait değil."
                );
            }

            if (oyunSoru.BaslangicZamani != default)
            {
                return Ok(new
                {
                    oyunId = oyunId,
                    soruId = soruId,
                    baslangicZamani = oyunSoru.BaslangicZamani,
                    sure = 15
                });
            }

            oyunSoru.BaslangicZamani = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                oyunId = oyunId,
                soruId = soruId,
                baslangicZamani = oyunSoru.BaslangicZamani,
                sure = 15
            });

         
        }

        [HttpPost("cevapla")]
        public async Task<IActionResult> Cevapla(
            OyunCevap cevap)
        {
            var kullaniciId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

            if (string.IsNullOrEmpty(kullaniciId))
            {
                return Unauthorized();
            }

            var oyun = await _context.Oyunlar
                .FirstOrDefaultAsync(x =>
                    x.Id == cevap.OyunId &&
                    x.KullaniciId == kullaniciId);

            if (oyun == null)
            {
                return NotFound("Oyun bulunamadı.");
            }

            var oyunSoru =
                await _context.OyunSorulari
                    .FirstOrDefaultAsync(x =>
                        x.OyunId == cevap.OyunId &&
                        x.SoruId == cevap.SoruId);

            if (oyunSoru == null)
            {
                return NotFound(
                    "Bu soru bu oyuna ait değil."
                );
            }

            var soru =
                await _context.Sorular
                    .FirstOrDefaultAsync(x =>
                        x.Id == cevap.SoruId);

            if (soru == null)
            {
                return NotFound(
                    "Soru bulunamadı."
                );
            }

            var dahaOnceCevaplandi =
                await _context.OyunCevaplari
                    .AnyAsync(x =>
                        x.OyunId == cevap.OyunId &&
                        x.SoruId == cevap.SoruId &&
                        x.Kullaniciİd == kullaniciId);

            if (dahaOnceCevaplandi)
            {
                return BadRequest(
                    "Bu soru daha önce cevaplandı."
                );
            }

            if (oyunSoru.BaslangicZamani == default)
            {
                return BadRequest(
                    "Bu soru henüz başlatılmadı."
                );
            }

            var gecenSure =
                DateTime.UtcNow -
                oyunSoru.BaslangicZamani;

            if (gecenSure.TotalSeconds > 15)
            {
                return BadRequest(
                    "Bu sorunun cevap süresi doldu."
                );
            }

            cevap.Kullaniciİd = kullaniciId;

            cevap.DogruMu =
                cevap.Cevap.Trim().Equals(
                    soru.DogruCevap.Trim(),
                    StringComparison.OrdinalIgnoreCase
                );

            if(!cevap.DogruMu )
            {
                cevap.Puan = 0;
            }
            else
            {
                var DogruCevapSayisi =
                    await _context.OyunCevaplari
                    .CountAsync(X =>
                    X.OyunId == cevap.OyunId &&
                    X.SoruId == cevap.SoruId &&
                    X.DogruMu);

                cevap.Puan = DogruCevapSayisi switch
                {
                    0 => 300,
                    1 => 280,
                    2 => 260,
                    3 => 240,
                    4 => 220,
                    5 => 200,
                    6 => 190,
                    7 => 180,
                    8 => 170,
                    9 => 160,
                    10 => 150,
                    11 => 150,
                    12 => 150,
                    13 => 150,
                    14 => 150,
         
                    _ =>0

                };




            }

            cevap.Tarih =
                DateTime.UtcNow;

            _context.OyunCevaplari.Add(cevap);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                dogruMu = cevap.DogruMu,
                puan = cevap.Puan,
                gecenSaniye =
                    Math.Round(
                        gecenSure.TotalSeconds,
                        2
                    )
            });
        
        
        }

        [HttpPost("{oyunId}/hazir")]
        public async Task<IActionResult> HazirOl(int oyunId)
        {
            var kullaniciId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(kullaniciId))
            {
                return Unauthorized();
            }

            var oyuncu = await _context.OyunOyunculari
                .FirstOrDefaultAsync(x =>
                    x.OyunId == oyunId &&
                    x.KullaniciId == kullaniciId);

            if (oyuncu == null)
            {
                return NotFound(
                    "Bu oyuncu bu oyunda bulunmuyor."
                );
            }

            oyuncu.HazirMi = true;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mesaj = "Hazır olduğunuz kaydedildi.",
                oyunId = oyunId,
                kullaniciId = kullaniciId,
                hazirMi = oyuncu.HazirMi
            });
        }
    }
}