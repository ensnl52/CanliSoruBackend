namespace CanliSoruBackend.Models
{
    public class OyunOyuncu
    {
        public int Id { get; set; }
        public int OyunId { get; set; }
        public string KullaniciId { get; set; } = "";
        public int Puan { get; set; } = 0;
        public Oyun Oyun { get; set; } = null!;
        public bool HazirMi { get; set; } = false;  

    }
}
