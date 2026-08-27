using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using CanliSoruBackend.Models;




namespace CanliSoruBackend.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {public AppDbContext(DbContextOptions<AppDbContext> options)
            :base (options) 
        {   
        }
        public DbSet<Soru> Sorular { get; set; }
        public DbSet<Oyun> Oyunlar { get; set; }

        public DbSet<OyunSoru> OyunSorulari { get; set; }

        public DbSet<OyunCevap> OyunCevaplari { get; set; }

        public DbSet<OyunOyuncu> OyunOyunculari { get; set; }
    }
}
