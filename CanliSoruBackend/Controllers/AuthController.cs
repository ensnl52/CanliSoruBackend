using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CanliSoruBackend.Models;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CanliSoruBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var kullanici = new ApplicationUser
            {
                UserName = request.KullaniciAdi,
                Email = request.Email,
                KullaniciAdi = request.KullaniciAdi
            };

            var sonuc = await _userManager.CreateAsync(
                kullanici,
                request.Sifre
            );

            if (!sonuc.Succeeded)
            {
                return BadRequest(sonuc.Errors);
            }

            return Ok("Kullanıcı başarıyla oluşturuldu");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var kullanici = await _userManager.FindByNameAsync(
                request.KullaniciAdi
            );

            if (kullanici == null)
            {
                return Unauthorized("Kullanıcı adı veya şifre hatalı.");
            }

            var sonuc = await _userManager.CheckPasswordAsync(
                kullanici,
                request.Sifre
            );

            if (!sonuc)
            {
                return Unauthorized("Kullanıcı adı veya şifre hatalı.");
            }

            var roller = await _userManager.GetRolesAsync(kullanici);

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    kullanici.Id
                ),

                new Claim(
                    ClaimTypes.Name,
                    kullanici.UserName ?? ""
                ),

                new Claim(
                    "KullaniciTipi",
                    "User"
                )
            };

            foreach (var rol in roller)
            {
                claims.Add(
                    new Claim(
                        ClaimTypes.Role,
                        rol
                    )
                );
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "CANLI-SORU-OYUNU-GIZLI-ANAHTAR-123456789"
                )
            );

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: "CanliSoruBackend",
                audience: "CanliSoruFrontend",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler()
                .WriteToken(token);

            return Ok(new
            {
                token = tokenString,
                kullaniciTipi = "User",
                roller = roller
            });
        }

        [HttpPost("guest")]
        public IActionResult Guest()
        {
            var guestId = Guid.NewGuid().ToString();

            var claims = new[]
            {
    new Claim(
        ClaimTypes.NameIdentifier,
        guestId
    ),

    new Claim(
        ClaimTypes.Name,
        "Guest"
    ),

    new Claim(
        ClaimTypes.Role,
        "Guest"
    ),

    new Claim(
        "KullaniciTipi",
        "Guest"
    )
};

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "CANLI-SORU-OYUNU-GIZLI-ANAHTAR-123456789"
                )
            );

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: "CanliSoruBackend",
                audience: "CanliSoruFrontend",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials
            );

            var tokenString =
                new JwtSecurityTokenHandler()
                    .WriteToken(token);

            return Ok(new
            {
                token = tokenString,
                kullaniciTipi = "Guest"
            });
        }

        [HttpGet("durum")]
        public IActionResult Durum()
        {
            var authorization =
                Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(authorization))
            {
                return Ok(new
                {
                    girisYapmis = false,
                    kullaniciTipi = "Guest"
                });
            }

            if (!authorization.StartsWith("Bearer "))
            {
                return Unauthorized();
            }

            var token =
                authorization.Substring("Bearer ".Length);

            var tokenHandler =
                new JwtSecurityTokenHandler();

            try
            {
                var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        "CANLI-SORU-OYUNU-GIZLI-ANAHTAR-123456789"
                    )
                );

                tokenHandler.ValidateToken(
                    token,
                    new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = key,

                        ValidateIssuer = true,
                        ValidIssuer = "CanliSoruBackend",

                        ValidateAudience = true,
                        ValidAudience = "CanliSoruFrontend",

                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    },
                    out SecurityToken validatedToken
                );

                var jwtToken =
                    (JwtSecurityToken)validatedToken;

                var kullaniciTipi =
                    jwtToken.Claims
                        .FirstOrDefault(
                            x => x.Type == "KullaniciTipi"
                        )
                        ?.Value;

                if (kullaniciTipi == "Guest")
                {
                    return Ok(new
                    {
                        girisYapmis = false,
                        kullaniciTipi = "Guest"
                    });
                }

                return Ok(new
                {
                    girisYapmis = true,
                    kullaniciTipi = "User"
                });
            }
            catch
            {
                return Unauthorized();
            }
        }

        [HttpPost("admin-yap")]
        public async Task<IActionResult> AdminYap(
            string kullaniciAdi)
        {
            var kullanici =
                await _userManager.FindByNameAsync(
                    kullaniciAdi
                );

            if (kullanici == null)
            {
                return NotFound("Kullanıcı bulunamadı.");
            }

            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(
                    new IdentityRole("Admin")
                );
            }

            var sonuc =
                await _userManager.AddToRoleAsync(
                    kullanici,
                    "Admin"
                );

            if (!sonuc.Succeeded)
            {
                return BadRequest(sonuc.Errors);
            }

            return Ok("Kullanıcı Admin yapıldı.");
        }
    }
}