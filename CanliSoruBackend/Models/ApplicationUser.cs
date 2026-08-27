using Microsoft.AspNetCore.Identity;
using CanliSoruBackend.Models;
using Microsoft.AspNetCore.Mvc;



namespace CanliSoruBackend.Models
{
    public class ApplicationUser : IdentityUser 
    {
        public string KullaniciAdi { get; set; } = "";

    }
}
