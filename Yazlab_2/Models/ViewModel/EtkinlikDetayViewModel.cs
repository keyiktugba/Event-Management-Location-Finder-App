namespace Yazlab_2.Models.ViewModel
{
    public class EtkinlikDetayViewModel
    {
        public string EtkinlikAdi { get; set; }
        public string Aciklama { get; set; }
        public DateTime Tarih { get; set; }
        public TimeSpan Saat { get; set; }
        public int EtkinlikSuresi { get; set; }
        public string KategoriAdi { get; set; }

     
        public string Konum { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string EtkinlikFoto { get; set; }
    }


}
