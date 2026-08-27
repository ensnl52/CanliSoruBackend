namespace CanliSoruBackend.Models
{
    public class OyunSoru
    {
        public int Id { get; set; }
        public  int OyunId { get; set; }
        public int SoruId { get; set; }
        public Oyun Oyun { get; set; } = null!;
        public Soru Soru { get; set; } = null!;
        public int Sira { get; set; }
        public DateTime  BaslangicZamani { get; set; }
    }
}
