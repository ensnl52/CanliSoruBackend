using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CanliSoruBackend.Data;
using CanliSoruBackend.Models;

namespace CanliSoruBackend.Controllers
{
    [Route("api/Soru")]
    [ApiController]
    public class SoruController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SoruController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> SorulariGetir()
        {
            var sorular = await _context.Sorular.ToListAsync();

            return Ok(sorular);
        }
        [HttpGet("rastgele")]
        public async Task<IActionResult> RastgeleSoru()
        {
            var soru = await _context.Sorular
                .OrderBy(x => Guid.NewGuid())
                .FirstOrDefaultAsync();

            if (soru == null)
            {
                return NotFound("Henüz soru bulunmuyor.");
            }

            return Ok(soru);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> SoruEkle(Soru soru)
        {
            _context.Sorular.Add(soru);

            await _context.SaveChangesAsync();

            return Ok(soru);
        }
    }
}   