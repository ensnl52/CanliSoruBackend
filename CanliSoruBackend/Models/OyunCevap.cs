namespace CanliSoruBackend.Models
{
    public class OyunCevap
    {
        public int Id { get; set; }
        public int OyunId { get; set; }
        public int SoruId { get; set; }
        public string Kullaniciİd { get; set; } = "";

        public string Cevap { get; set; } = "";

        public bool DogruMu { get; set; }
        public int Puan { get; set; }
        public DateTime Tarih { get; set; } = DateTime.UtcNow;
    }
}
