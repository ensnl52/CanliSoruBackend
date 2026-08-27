namespace CanliSoruBackend.Models
{
    public class Oyun
    {
        public int Id { get; set; }
        public string OdaKodu { get; set; } = "";
        public string KullaniciId { get; set; } = "";

        public DateTime Tarih { get; set; }

        public DateTime BaslangicZamani { get; set; } = DateTime.UtcNow;
        public ICollection<OyunSoru> OyunSorulari { get; set; } = new List<OyunSoru>();

        public ICollection<OyunOyuncu> Oyuncular { get; set; } = new List<OyunOyuncu>();

    }
}
