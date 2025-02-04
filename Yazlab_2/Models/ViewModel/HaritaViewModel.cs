namespace Yazlab_2.Models.ViewModel
{

    public class HaritaViewModel
    {
        public string EtkinlikAdi { get; set; }
        public string Aciklama { get; set; }
        public string EtkinlikKonum { get; set; }  // Etkinlik konumunu ekliyoruz
        public string KullaniciKonum { get; set; }  // Kullanıcı konumunu ekliyoruz
        public string Foto { get; set; }
        public DateTime Tarih { get; set; } // Etkinlik tarihi
        public TimeSpan Saat { get; set; } // Etkinlik saati
        public int EtkinlikSuresi { get; set; } // Etkinlik süresi
    }



}